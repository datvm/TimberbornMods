namespace ScrollableEntityPanel;

[Context("Game")]
[Context("MapEditor")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<ScollAdder>().AsSingleton();
    }
}
