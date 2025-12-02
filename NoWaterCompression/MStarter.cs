namespace NoWaterCompression;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(NoWaterCompression)).PatchAll();
    }
}
