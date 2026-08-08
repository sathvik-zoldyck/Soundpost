using Soundpost.Core.Storage;

namespace Soundpost.App.State;

/// <summary>
/// What the console remembers between runs. Kept intentionally small; it grows as features that
/// need to persist (scenes, per-app device memory, preferences) come online.
/// </summary>
public sealed class AppState : ISchemaVersioned
{
    public int SchemaVersion { get; set; } = 1;

    // Window placement. NaN position means "never saved" — the window centres itself instead.
    public double WindowWidth { get; set; }
    public double WindowHeight { get; set; }
    public double WindowLeft { get; set; } = double.NaN;
    public double WindowTop { get; set; } = double.NaN;
    public bool WindowMaximized { get; set; }

    /// <summary>The section the console was showing, restored on next launch.</summary>
    public string LastSection { get; set; } = "Dashboard";

    /// <summary>The active colour theme: "Indigo" (default) or "BlackRed".</summary>
    public string Theme { get; set; } = "Indigo";
}
