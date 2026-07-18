namespace HousePainter;

/// <summary>
/// Game DI bindings via attributes, plus Harmony PatchAll for this assembly.
/// </summary>
public class MConfigs : BaseModdableTimberbornAttributeConfiguration, IModdableTimberbornRegistryWithPatchConfig
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    /// <summary>null → PatchAll on this assembly (AutoAtlasingPatches).</summary>
    public string? PatchCategory { get; }
}
