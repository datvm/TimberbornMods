namespace ModdableTimberborn;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var h = new Harmony(nameof(ModdableTimberborn));
        h.PatchAll();
    }

}
