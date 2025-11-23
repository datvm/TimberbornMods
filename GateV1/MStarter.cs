namespace GateV1;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(GateV1)).PatchAll();
    }
}
