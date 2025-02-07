namespace UnlimitedMapSize;
internal class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony(nameof(UnlimitedMapSize));
        harmony.PatchAll();
    }

}