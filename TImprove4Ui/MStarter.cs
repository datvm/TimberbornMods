namespace TImprove4Ui;

public class MStarter : IModStarter
{
    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony(nameof(TImprove4Ui));
        harmony.PatchAll();

        harmony.Patch(
            typeof(TopBarCounterRow).GetConstructors().First(),
            postfix: typeof(MaterialCounterPatches).Method(nameof(MaterialCounterPatches.AddCounterEvents))
        );

    }
}
