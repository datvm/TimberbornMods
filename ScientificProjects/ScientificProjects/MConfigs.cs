namespace ScientificProjects;

public class MConfigs : BaseModdableTimberbornConfigurationWithHarmony
{

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
        }

        if (!context.IsGameContext()) { return; }

        configurator
            .BindSingleton<ScientificProjectUnlockRegistry>()
            .BindSingleton<ScientificProjectRegistry>()
            .BindSingleton<ScientificProjectUnlockService>()
            .BindSingleton<ScientificProjectDailyService>()
            .BindSingleton<ScientificProjectGroupService>()
            .BindSingleton<ScientificProjectService>()            

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
            .MultiBindSingleton<IProjectCostProvider, ModProjectsCostProvider>()
            .MultiBindSingleton<ICharacterUpgradeDescriber, ModProjectsCharacterDescriber>()
            .MultiBindSingleton<IWorkplaceUpgradeDescriber, ModProjectsWorkplaceDescriber>()
            .MultiBindSingleton<ISPDevModule, DefaultSpDevModule>()

            .MultiBindSingleton<ISpecModifier, FactionUpgradeRecipeModifier>()
            .MultiBindSingleton<IPrefabModifier, FactionUpgradePrefabModifier>()

            .BindSingleton<ModUpgradeListener>()

            .BindTemplateModule(h => h
                .AddDecorator<Character, CharacterProjectUpgradeDescriber>()
                .AddDecorator<WorkplaceSpec, WorkplaceProjectUpgradeDescriber>()

                .AddDecorator<SPFactionUpgradeDescriberSpec, SPFactionUpgradeDescriber>()
            )
        ;
    }

}