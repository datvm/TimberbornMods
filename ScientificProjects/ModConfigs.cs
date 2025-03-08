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

        Bind<OneTimeUnlockProcessor>().AsSingleton();
        BindBuffStuff();
    }

    void BindBuffStuff()
    {
        // Buffs
        Bind<ResearchProjectsBuff>().AsSingleton();

        // Components
        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<BeaverSpec, BeaverBuffComponent>();
            b.AddDecorator<Character, CharacterBuffComponent>();
            return b.Build();
        }).AsSingleton();
    }

}