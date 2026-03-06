global using PausableSensors.Components;

namespace PausableSensors;

[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<AutomatorPausable>().AsTransient();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            var b = new TemplateModule.Builder();
            b.AddDecorator<ITransmitter, AutomatorPausable>();
            return b.Build();
        }).AsSingleton();
    }
}
