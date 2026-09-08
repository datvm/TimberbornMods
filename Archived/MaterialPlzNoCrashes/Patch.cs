using System.Reflection.Emit;

namespace MaterialPlzNoCrashes;

[HarmonyPatch]
public class Patch
{

    static readonly MethodInfo PrintWarningInsteadMethod = typeof(Patch).Method(nameof(PrintWarningInstead));

    static void PrintWarningInstead(object ex)
    {
        Debug.LogWarning("Duplicate materials detected: " + ex.ToString());
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(MaterialRepository), nameof(MaterialRepository.Load))]
    public static IEnumerable<CodeInstruction> PatchLoadDuplicateMaterials(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var ins in instructions)
        {
            if (ins.opcode == OpCodes.Throw)
            {
                // Call Debug.LogWarning instead
                yield return new CodeInstruction(OpCodes.Call, PrintWarningInsteadMethod);
            }
            else
            {
                yield return ins;
            }
        }
    }

}

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(MaterialPlzNoCrashes)).PatchAll();
    }

}

