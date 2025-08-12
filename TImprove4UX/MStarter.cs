namespace TImprove4UX;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(TImprove4UX)).PatchAll();
    }

}
