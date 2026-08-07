using System.Text.Json;

namespace Soundpost.Core.Storage;

/// <summary>
/// A crash-safe JSON config store. Writes go to a temp file first and then atomically replace the
/// target via <see cref="File.Replace(string, string, string?)"/>, which also rotates the previous
/// good copy into a <c>.bak</c> — so an interrupted write can never leave a truncated file behind.
/// Reads fall back to the backup, then to defaults, and quarantine anything unreadable rather than
/// throwing at the caller. Optionally migrates files written by an older schema version.
/// </summary>
/// <typeparam name="T">The settings record. If it implements <see cref="ISchemaVersioned"/>, the
/// store stamps the current version on save and runs <paramref name="migrate"/> on older files.</typeparam>
public sealed class AtomicJsonStore<T> : IConfigStore<T>
    where T : class, new()
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _path;
    private readonly string _backupPath;
    private readonly string _tempPath;
    private readonly int _currentVersion;
    private readonly Func<T, int, T>? _migrate;
    private readonly object _gate = new();

    /// <param name="path">Full path to the JSON file. Its directory is created on first save.</param>
    /// <param name="currentVersion">Schema version this build writes (used only for <see cref="ISchemaVersioned"/> types).</param>
    /// <param name="migrate">Given a loaded value and the version it was written with, return an upgraded value.</param>
    public AtomicJsonStore(string path, int currentVersion = 1, Func<T, int, T>? migrate = null)
    {
        _path = path;
        _backupPath = path + ".bak";
        _tempPath = path + ".tmp";
        _currentVersion = currentVersion;
        _migrate = migrate;
    }

    public T Load()
    {
        lock (_gate)
        {
            return TryRead(_path, out T? primary) && primary is not null
                ? Migrate(primary)
                : TryRead(_backupPath, out T? backup) && backup is not null
                    ? Migrate(backup)
                    : new T();
        }
    }

    public void Save(T value)
    {
        lock (_gate)
        {
            if (value is ISchemaVersioned versioned)
            {
                versioned.SchemaVersion = _currentVersion;
            }

            string? directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write the new contents fully to a temp file and flush to disk before touching the
            // real file, so a crash here leaves the existing good file untouched.
            using (var stream = new FileStream(_tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, value, Options);
                stream.Flush(flushToDisk: true);
            }

            if (File.Exists(_path))
            {
                // Atomic on the same volume: swaps temp into place and rotates the old file to .bak.
                File.Replace(_tempPath, _path, _backupPath, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(_tempPath, _path);
            }
        }
    }

    private bool TryRead(string path, out T? value)
    {
        value = null;
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            using FileStream stream = File.OpenRead(path);
            value = JsonSerializer.Deserialize<T>(stream, Options);
            return value is not null;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            // Corrupt or unreadable — set it aside (best effort) so the next save starts clean and
            // the bad file is preserved for inspection rather than silently overwritten.
            Quarantine(path);
            return false;
        }
    }

    private static void Quarantine(string path)
    {
        try
        {
            File.Move(path, path + ".corrupt", overwrite: true);
        }
        catch
        {
            // If we can't move it, leave it; the loader already fell through to defaults.
        }
    }

    private T Migrate(T value)
    {
        if (value is ISchemaVersioned versioned && versioned.SchemaVersion != _currentVersion && _migrate is not null)
        {
            T upgraded = _migrate(value, versioned.SchemaVersion);
            if (upgraded is ISchemaVersioned v)
            {
                v.SchemaVersion = _currentVersion;
            }

            return upgraded;
        }

        return value;
    }
}
