global using MoreOverlay.Components;
global using MoreOverlay.Services;

namespace MoreOverlay;


public class MMoreOverlayConfig : BaseModdableTimberbornAttributeConfiguration, IModdableTimberbornRegistryWithPatchConfig
{
    public string? PatchCategory { get; }

    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

}