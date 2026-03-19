namespace TestMod;

[Context("Bootstrapper")]
public class ModBootstrapperConfig : Configurator
{
    public override void Configure()
    {

    }
}

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<Test>().AsSingleton();
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
    }
}

class Test : ILoadableSingleton
{
    public void Load()
    {
    }
}