namespace ScientificProjects;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<ScientificProjectRegistry>().AsSingleton();
        Bind<ScientificProjectService>().AsSingleton();

        Bind<ScientificProjectScreen>().AsSingleton();
        
        MultiBind<IProjectCostProvider>().To<ModProjectCostProvider>().AsSingleton();
        MultiBind<IProjectUnlockConditionProvider>().To<ModProjectUnlockConditionProvider>().AsSingleton();
        MultiBind<IDevModule>().To<ScientificProjectDevModule>().AsSingleton();

        Bind<OneTimeUnlockProcessor>().AsSingleton();
        BindBuffStuff();
    }

    void BindBuffStuff()
    {
        // Buffs
        Bind<ResearchProjectsBuff>().AsSingleton();

        // Components
        this.BindTemplateModule()
            .AddDecorator<BeaverSpec, BeaverBuffComponent>()
            .AddDecorator<Character, CharacterBuffComponent>()
            .AddDecorator<Manufactory, ManufactoryBuffComponent>()
            .Bind();

        // Type Trackers
        this.BindTrackingEntities()
            .TrackBuilderBuildings()
            .TrackManufactory()
            .TrackWorkplace();
    }

}