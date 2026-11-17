namespace MechanizedManufactory.Patches;

[HarmonyPatch]
public static class StafflessWorkplacePatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(NoUnemployedStatus), nameof(NoUnemployedStatus.UpdateStatus))]
    public static bool RemoveNoUnemployedStatus(NoUnemployedStatus __instance)
        => __instance.GetComponent<MechanizedManufactorySpec>() is null;

    [HarmonyPrefix, HarmonyPatch(typeof(WorkplaceFragment), nameof(WorkplaceFragment.ShowFragment))]
    public static bool DoNotShowWorkerUI(BaseComponent entity)
        => entity.GetComponent<MechanizedManufactorySpec>() is null;

    [HarmonyPrefix, HarmonyPatch(typeof(WorkplaceBatchControlRowItemFactory), nameof(WorkplaceBatchControlRowItemFactory.Create))]
    public static bool DoNotShowBatchControlUI(BaseComponent entity, ref IBatchControlRowItem? __result)
    {
        if (!entity.HasComponent<MechanizedManufactorySpec>()) { return true; }

        __result = null;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(WorkplaceDescriber), nameof(WorkplaceDescriber.DescribeEntity))]
    public static bool DoNotShowWorkerDescriber(WorkplaceDescriber __instance, ref IEnumerable<EntityDescription> __result)
    {
        if (__instance.GetComponent<MechanizedManufactorySpec>() is null) { return true; }

        __result = [];
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ManufactoryDescriber), nameof(ManufactoryDescriber.GetCraftingTime))]
    public static void SetWorkerCount(ManufactoryDescriber __instance, ref float workers)
    {
        if (workers > 0f) { return; }
        if (__instance.GetComponent<MechanizedManufactorySpec>() is not { Workers: var count }) { return; }

        workers = count;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ProductionProgressFragment), nameof(ProductionProgressFragment.UpdateCraftingTime))]
    public static bool UpdateCraftingTime(ProductionProgressFragment __instance)
    {
        var wp = __instance._workplace;
        if (!wp || wp.DesiredWorkers > 0) { return true; }
        if (wp.GetComponent<MechanizedManufactorySpec>() is not { Workers: var workers }) { return true; }

        __instance._craftingTime.text = __instance._manufactory.HasCurrentRecipe
            ? __instance._manufactoryDescriber.GetCraftingTime(__instance._manufactory.CurrentRecipe, workers)
            : "-";
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(WorkplaceIlluminator), nameof(WorkplaceIlluminator.Awake))]
    public static void DoNotTurnBasedOnWorkers(WorkplaceIlluminator __instance)
    {
        if (__instance.HasComponent<MechanizedManufactorySpec>())
        {
            __instance.DisableComponent();
        }
    }

}