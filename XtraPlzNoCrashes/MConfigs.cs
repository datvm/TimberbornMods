namespace XtraPlzNoCrashes;

[Context("MainMenu")]
public class MMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<ModVerifier>().AsSingleton();
    }
}
