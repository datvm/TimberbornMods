namespace ScientificProjects.Patches;

[HarmonyPatch]
public static class WheelbarrowPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(GoodCarrier), nameof(GoodCarrier.IsMovementSlowed), MethodType.Getter)]
    public static bool PatchWheelbarrow(ref bool __result)
    {
        if (ModUpgradeListener.WheelbarrowUnlocked)
        {
            __result = false;
            return false;
        }

        return true;
    }

}
