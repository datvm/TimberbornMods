namespace DisableHauling;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration, IModdableTimberbornRegistryWithPatchConfig
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    public string? PatchCategory { get; }
}
