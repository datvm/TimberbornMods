namespace ScientificProjects;

public class MConfigs : BaseModdableTimberbornConfigurationWithHarmony, IWithDIConfig
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game | ConfigurationContext.MainMenu;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        ModdableTimberbornRegistry.Instance
            .UseBonusTracker()
            .UseEntityTracker();

        base.StartMod(modEnvironment);
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (context.IsMenuContext())
        {
            configurator
                .MultiBindSingleton<IModUpdateNotifier, ModUpdateNotifier>()
            ;

            return;
        }

        configurator
            .BindSingleton<ScientificProjectUnlockRegistry>()
            .BindSingleton<ScientificProjectRegistry>()
            .BindSingleton<ScientificProjectUnlockService>()
            .BindSingleton<ScientificProjectDailyService>()
            .BindSingleton<ScientificProjectGroupService>()
            .BindSingleton<ScientificProjectService>()
            .BindSingleton<ScientificProjectStateListener>()

            // Project upgrades
            .BindSingleton<EntityUpgradeDescriber>()

            // Dialog
            .BindSingleton<ScientificProjectDialogController>()
            .BindTransient<ScientificProjectDialog>()
            .BindTransient<SPDevPanel>()
            .BindTransient<SPSciencePanel>()
            .BindTransient<SPListElement>()
            .BindTransient<SPGroupElement>()
            .BindTransient<SPElement>()

            // Base mod processings
            .BindScientificProjectCostProvider<ModProjectsCostProvider>()
            .MultiBindSingleton<ICharacterUpgradeDescriber, ModProjectsCharacterDescriber>()
            .MultiBindSingleton<IWorkplaceUpgradeDescriber, ModProjectsWorkplaceDescriber>()
            .MultiBindSingleton<ISPDevModule, DefaultSpDevModule>()

            .BindSpecModifier<FactionUpgradeRecipeModifier>()
            .BindTemplateModifier<FactionUpgradeTemplateModifier>()

            .BindSingleton<ModUpgradeListener>()

            .BindTemplateModule(h => h
                .AddDecorator<Character, CharacterProjectUpgradeDescriber>()
                .AddDecorator<WorkplaceSpec, WorkplaceProjectUpgradeDescriber>()

                .AddDecorator<SPFactionUpgradeDescriberSpec, SPFactionUpgradeDescriber>()
            )
        ;
    }

}