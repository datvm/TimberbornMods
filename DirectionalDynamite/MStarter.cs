namespace DirectionalDynamite;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(DirectionalDynamite)).PatchAll();
    }

}
