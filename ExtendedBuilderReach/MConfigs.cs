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

        Bind<ExtendedGoodStackAccessible>().AsTransient();
        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<GoodStackAccessible, ExtendedGoodStackAccessible>();
            return b.Build();
        }).AsSingleton();
    }
}
