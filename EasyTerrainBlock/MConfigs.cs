global using EasyTerrainBlock.Services;

namespace EasyTerrainBlock;

public class MEasyTerrainBlockConfig : BaseModdableTimberbornAttributeConfiguration, IModdableTimberbornRegistryWithPatchConfig
{
    public string? PatchCategory { get; }
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;
}
