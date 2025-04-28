global using NoBuildRestriction.Patches;

namespace NoBuildRestriction;

[Context("MainMenu")]
public class MenuModConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

[Context("Game")]
public class GameModConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}


public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony(nameof(NoBuildRestriction));
        harmony.PatchAll();
    }

}