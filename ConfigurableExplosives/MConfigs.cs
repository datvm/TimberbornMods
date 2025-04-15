global using ConfigurableExplosives.Patches;

namespace ConfigurableExplosives;

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

        this.MultiBindAndBindSingleton<IPrefabModifier, PrefabPatches>();
    }

}
