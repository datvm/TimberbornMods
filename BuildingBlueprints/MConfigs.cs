namespace BuildingBlueprints;

public class MGameConfig : BaseModdableTimberbornAttributeConfiguration, IModdableTimberbornRegistryWithPatchConfig
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game | ConfigurationContext.MainMenu;
    public string? PatchCategory => null;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseBuildingSettings();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        if (context != ConfigurationContext.Game) { return; }

        configurator
            .TryBindingSystemFileDialogService()

            .MultiBindCustomTool<BuildingBlueprintsButtons>()
        ;
    }
}