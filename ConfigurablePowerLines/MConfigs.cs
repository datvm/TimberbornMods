namespace ConfigurablePowerLines;

[Context("MainMenu")]
[Context("Game")]
public class CommonConfig : Configurator
{

    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }

}
