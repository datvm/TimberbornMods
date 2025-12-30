namespace WarningsBeGone;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(WarningsBeGone)).PatchAll();
    }
}
