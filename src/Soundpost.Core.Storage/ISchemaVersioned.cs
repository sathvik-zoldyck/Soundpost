namespace Soundpost.Core.Storage;

/// <summary>
/// A settings record that carries its own schema version, so the store can recognise and migrate
/// files written by an older build instead of silently misreading them.
/// </summary>
public interface ISchemaVersioned
{
    /// <summary>The schema version this instance was written with. The store stamps it on save.</summary>
    int SchemaVersion { get; set; }
}
