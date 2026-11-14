namespace BeaverBellStatue;

[Context("Game")]
public class MGameConfig : Configurator
{

    public override void Configure()
    {
        Bind<BeaverBellComponent>().AsTransient();
        MultiBind<TemplateModule>().ToProvider(() =>
        {
            var b = new TemplateModule.Builder();
            b.AddDecorator<BeaverBellSpec, BeaverBellComponent>();
            return b.Build();
        }).AsSingleton();
    }

}
