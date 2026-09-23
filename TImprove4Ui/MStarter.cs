namespace TImprove4Ui;

public class MStarter : IModStarter
{
    public static bool HasMiniMap { get; private set; } = false;

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
