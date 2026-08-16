namespace Soundpost.Plugin.Abstractions;

/// <summary>
/// Writes to the app log, tagged with the plugin's id. Prefer this over <c>Console</c>/<c>Debug</c> so a
/// user's log tells them which plugin said what. Never log secrets or PII.
/// </summary>
public interface IPluginLog
{
    /// <summary>Informational message.</summary>
    void Info(string message);

    /// <summary>Something unexpected but recoverable.</summary>
    void Warn(string message);

    /// <summary>A failure, optionally with the exception that caused it.</summary>
    void Error(string message, Exception? exception = null);
}
