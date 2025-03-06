
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

        BindBuffStuff();
    }

    void BindBuffStuff()
    {
        // Buffs
        Bind<MoveSpeedBuff>().AsSingleton();
        Bind<WorkEffBuff>().AsSingleton();

        // Components
        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<BeaverSpec, BeaverBuffComponent>();
            return b.Build();
        }).AsSingleton();
    }

}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var h = new Harmony(nameof(ScientificProjects));
        h.PatchAll();
    }

}