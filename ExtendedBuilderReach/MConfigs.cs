global using ExtendedBuilderReach.Components;

namespace ExtendedBuilderReach;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }

}

[Context("Game")]
public class ModGameConfig : Configurator
{

    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();

        Bind<ExtendedDemolishableAccessible>().AsTransient();
        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<Demolishable, ExtendedDemolishableAccessible>();
            return b.Build();
        }).AsSingleton();
    }
}
