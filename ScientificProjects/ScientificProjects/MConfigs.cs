namespace ScientificProjects;

public class MConfigs : BaseModdableTimberbornConfiguration
{

    public override void StartMod(IModEnvironment modEnvironment)
    {
        ModdableTimberbornRegistry.Instance
            .UseBonusTracker();

        base.StartMod(modEnvironment);
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindSingleton<ScientificProjectUnlockRegistry>()
            .BindSingleton<ScientificProjectRegistry>()
            .BindSingleton<ScientificProjectUnlockService>()
            .BindSingleton<ScientificProjectDailyService>()
            .BindSingleton<ScientificProjectGroupService>()
            .BindSingleton<ScientificProjectService>()            

            // Project upgrades
            .BindSingleton<CharacterTracker>()
            .BindSingleton<WorkplaceTracker>()
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
            .MultiBindSingleton<ISPDevModule, DefaultSpDevModule>()
            .BindSingleton<ModUpgradeListener>()

            .BindTemplateModule(h => h
                .AddDecorator<Character, CharacterProjectUpgradeComponent>()
                .AddDecorator<Workplace, WorkplaceProjectUpgradeComponent>()
            )
        ;
    }

}