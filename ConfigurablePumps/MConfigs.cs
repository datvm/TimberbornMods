
namespace ConfigurablePumps;

public class MConfig : BaseModdableTimberbornAttributeConfiguration, IHarmonyPatchAll, IWithDIConfig
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.MainMenu | ConfigurationContext.Game;
}