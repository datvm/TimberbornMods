namespace MultiYieldersPlzNoCrashes;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(MultiYieldersPlzNoCrashes)).PatchAll();
    }
}