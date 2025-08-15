namespace TImprove4UX;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        Harmony harmony = new(nameof(TImprove4UX));
        harmony.PatchAll();
    }

}
