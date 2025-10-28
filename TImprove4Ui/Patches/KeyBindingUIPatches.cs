namespace TImprove4Ui.Patches;

[HarmonyPatch]
public static class KeyBindingUIPatches
{

    [HarmonyTranspiler, HarmonyPatch(typeof(KeyBindingRowFactory), nameof(KeyBindingRowFactory.CreateAll))]
    public static IEnumerable<CodeInstruction> PatchHiddenGroup(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        var method = typeof(KeyBindingGroupSpec).PropertyGetter(nameof(KeyBindingGroupSpec.IsHiddenGroup));

        foreach (var ins in instructions)
        {
            if (ins.Calls(method))
            {
                found = true;
                yield return new(OpCodes.Pop);
                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
            }
            else
            {
                yield return ins;
            }
        }

        if (!found)
        {
            throw new Exception("Failed to patch KeyBindingRowFactory.CreateAll");
        }
    }

}
