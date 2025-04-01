namespace WaterSwap;

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(WaterSwap)).PatchAll();
    }

}
