namespace WaterSourcesDontCrash;
internal class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(WaterSourcesDontCrash)).PatchAll();
    }

}
