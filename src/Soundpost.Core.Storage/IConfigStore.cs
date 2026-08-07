namespace Soundpost.Core.Storage;

/// <summary>
/// Loads and saves a single piece of application state as JSON. Implementations must never lose
/// good data to a crash mid-write, and must degrade to sensible defaults rather than throwing when
/// the file is missing or corrupt.
/// </summary>
/// <typeparam name="T">A serialisable settings record with a parameterless constructor.</typeparam>
public interface IConfigStore<T>
    where T : class, new()
{
    /// <summary>The value from disk, the backup if the primary is corrupt, or defaults if neither loads.</summary>
    T Load();

    /// <summary>Persist <paramref name="value"/> atomically, keeping the previous version as a backup.</summary>
    void Save(T value);
}
