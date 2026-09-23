namespace PowerOverdrive;

public class MConfig : BaseModdableTimberbornAttributeConfiguration, IHarmonyPatchAll
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance.UseMechanicalSystem();
    }
}
