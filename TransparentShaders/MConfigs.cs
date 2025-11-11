global using TransparentShaders.Services;

namespace TransparentShaders;

[Context("Bootstrapper")]
public class ModBootstrapperConfig : Configurator
{
    public override void Configure()
    {
        Bind<TransparentShaderService>().AsSingleton().AsExported();
    }
}

[Context("MainMenu")]
[Context("Game")]
[Context("MapEditor")]
public class ModAllContextConfig : Configurator
{
    public override void Configure()
    {
        Bind<TransparentShaderServiceUnloader>().AsSingleton();
    }
}