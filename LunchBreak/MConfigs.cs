global using LunchBreak.UI;
global using LunchBreak.Services;

namespace LunchBreak;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<LunchBreakManager>().AsSingleton();
        Bind<LunchBreakClockPanel>().AsSingleton();
        Bind<LunchBreakTimePanel>().AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(LunchBreak)).PatchAll();
    }

}
