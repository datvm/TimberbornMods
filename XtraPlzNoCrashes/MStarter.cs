namespace XtraPlzNoCrashes;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment) => new Harmony(nameof(XtraPlzNoCrashes)).PatchAll();
}
