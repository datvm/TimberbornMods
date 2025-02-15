namespace NoWaterCompression;
public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        Harmony harmony = new(nameof(NoWaterCompression));
        harmony.PatchAll();
    }

}
