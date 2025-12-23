namespace TestMod;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var h = new Harmony(nameof(TestMod));
        h.PatchAll();
    }


}

[HarmonyPatch]
public static class RemoveWarningLogPatch
{

    [HarmonyTranspiler, HarmonyPatch(typeof(PathMeshDrawer), nameof(PathMeshDrawer.Add))]
    public static IEnumerable<CodeInstruction> DontPrintLog(IEnumerable<CodeInstruction> instructions)
    {
        var target = typeof(Debug).Method(nameof(Debug.LogWarning), [typeof(object)]);

        foreach (var ins in instructions)
        {
            yield return ins.Calls(target) ? new(System.Reflection.Emit.OpCodes.Pop) : ins;
        }
    }
}