namespace MechanizedManufactory.Patches;

[HarmonyPatch(typeof(Workshop))]
public static class StafflessWorkshopPatches
{

    static readonly ConditionalWeakTable<Workshop, MechanizedManufactoryComponent> mechanizedWorkshops = [];

    [HarmonyPostfix, HarmonyPatch(nameof(Workshop.Awake))]
    public static void Mark(Workshop __instance)
    {
        var mm = __instance.GetComponent<MechanizedManufactoryComponent>();
        if (!mm) { return; }
        mechanizedWorkshops.Add(__instance, mm);
    }

    [HarmonyPrefix, HarmonyPatch(nameof(Workshop.CurrentlyWorking), MethodType.Getter)]
    public static bool IsMechanizedWorkshop(Workshop __instance, ref bool __result)
    {
        if (!mechanizedWorkshops.TryGetValue(__instance, out var mm))
        {
            return true;
        }

        __result = mm.IsWorking;
        return false;
    }

}
