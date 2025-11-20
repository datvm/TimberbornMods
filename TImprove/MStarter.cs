namespace TImprove;

public class MStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(TImprove)).PatchAll();
    }

}
