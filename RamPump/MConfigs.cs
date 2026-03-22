global using RamPump.Components;
global using RamPump.Services;

namespace RamPump;

[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<RamPumpService>().AsSingleton();

        Bind<RamPumpComponent>().AsTransient();
        Bind<RamPumpModelModifier>().AsTransient();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            var b = new TemplateModule.Builder();
            b.AddDecorator<RamPumpSpec, RamPumpComponent>();
            b.AddDecorator<RamPumpComponent, RamPumpModelModifier>();
            return b.Build();
        }).AsSingleton();
    }
}
