namespace Soundpost.Plugin.Abstractions;

/// <summary>
/// A small, JSON-backed key/value store, private to one plugin (kept under its
/// <see cref="ISoundpostHost.DataDirectory"/>). Values are strings; parse richer types yourself, or use
/// the helpers in <see cref="PluginStorageExtensions"/>. Keep it small — it is for preferences and a
/// little state, not a database.
/// </summary>
public interface IPluginStorage
{
    /// <summary>The stored value for <paramref name="key"/>, or <c>null</c> if absent.</summary>
    string? Get(string key);

    /// <summary>Store (or overwrite) a value.</summary>
    void Set(string key, string value);

    /// <summary>Remove a key. Returns <c>true</c> if it existed.</summary>
    bool Remove(string key);

    /// <summary>All keys currently stored.</summary>
    IReadOnlyCollection<string> Keys { get; }
}

/// <summary>Typed convenience over <see cref="IPluginStorage"/>'s string values.</summary>
public static class PluginStorageExtensions
{
    /// <summary>The stored string, or <paramref name="fallback"/> if the key is absent.</summary>
    public static string GetOrDefault(this IPluginStorage storage, string key, string fallback) =>
        storage.Get(key) ?? fallback;

    /// <summary>The stored value parsed as an <see cref="int"/>, or <paramref name="fallback"/>.</summary>
    public static int GetInt(this IPluginStorage storage, string key, int fallback) =>
        int.TryParse(storage.Get(key), out int value) ? value : fallback;

    /// <summary>The stored value parsed as a <see cref="double"/> (invariant), or <paramref name="fallback"/>.</summary>
    public static double GetDouble(this IPluginStorage storage, string key, double fallback) =>
        double.TryParse(
            storage.Get(key),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out double value)
            ? value
            : fallback;

    /// <summary>The stored value parsed as a <see cref="bool"/>, or <paramref name="fallback"/>.</summary>
    public static bool GetBool(this IPluginStorage storage, string key, bool fallback) =>
        bool.TryParse(storage.Get(key), out bool value) ? value : fallback;
}
