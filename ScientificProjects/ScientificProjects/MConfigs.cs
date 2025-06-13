namespace ScientificProjects;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<ScientificProjectUnlockManager>()
            .BindSingleton<ScientificProjectRegistry>()
            .BindSingleton<ScientificProjectService>()

            .BindSingleton<ScientificProjectScreen>()

            .MultiBindSingleton<IProjectCostProvider, ModProjectCostProvider>()

            .BindSingleton<OneTimeUnlockProcessor>()
        ;

        BindBuffStuff();
    }

    void BindBuffStuff()
    {
        this
            // Buffs
            .BindSingleton<ResearchProjectsBuff>()

            // Components
            .BindTemplateModule(h => h
                .AddDecorator<BeaverSpec, BeaverBuffComponent>()
                .AddDecorator<Character, CharacterBuffComponent>()
                .AddDecorator<Manufactory, ManufactoryBuffComponent>()
            )

            // Tracking entities
            .BindTrackingEntities()
                .TrackBuilderBuildings()
                .TrackManufactory()
                .TrackWorkplace()
        ;
    }

}