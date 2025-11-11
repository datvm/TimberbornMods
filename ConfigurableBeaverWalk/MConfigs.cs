namespace ConfigurableBeaverWalk;

[Context("MainMenu")]
[Context("Game")]
public class MConfigs : Configurator
{

    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }

}
