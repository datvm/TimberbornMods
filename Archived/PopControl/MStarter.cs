namespace PopControl;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(PopControl)).PatchAll();
    }

}
