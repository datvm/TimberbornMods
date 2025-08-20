namespace HangingOverhang;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(HangingOverhang)).PatchAll();
    }

}
