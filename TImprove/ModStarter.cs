namespace TImprove;

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(TImprove)).PatchAll();
    }

}
