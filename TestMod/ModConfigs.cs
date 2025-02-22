namespace TestMod;

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(TestMod)).PatchAll();
    }

}
