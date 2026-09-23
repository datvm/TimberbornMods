namespace GlobalInventory;

public class MConfig : BaseModdableTimberbornAttributeConfiguration, IHarmonyPatchAll
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;
}
