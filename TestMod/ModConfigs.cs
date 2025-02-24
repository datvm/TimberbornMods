namespace TestMod;

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony(nameof(TestMod));
        harmony.PatchAll();

    }

}
