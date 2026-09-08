namespace Packager;

public class MStarter : IModStarter
{
    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(Packager)).PatchAll();
    }
}
