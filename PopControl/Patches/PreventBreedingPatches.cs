namespace PopControl.Patches;

[HarmonyPatch]
public static class PreventBreedingPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(ProcreationHouse), nameof(ProcreationHouse.HasFreeChildSlot))]
    public static void PreventFolktailsProcreate(ref bool __result, ProcreationHouse __instance)
    {
        if (__result && PopControlService.Instance?.ShouldPreventBreeding(__instance) == true)
        {
            __result = false;
        }
    }

}
