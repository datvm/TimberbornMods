namespace ConstructionSiteHauler.Patches;

/// <summary>
/// When material hauling is disabled on the builders' hut and/or the construction site,
/// builders only hammer (ReadyToBuild path); they do not fetch materials.
/// </summary>
[HarmonyPatch]
public static class BuilderPatches
{
    /// <summary>
    /// Skip stock search / material reservation when either workplace or site disables hauling.
    /// Vanilla still runs ReadyToBuild / BuildBehavior first.
    /// </summary>
    [HarmonyPrefix, HarmonyPatch(typeof(ConstructionJob), nameof(ConstructionJob.ClosestObjectWithNeededGood))]
    public static bool SkipMaterialSearchWhenDisabled(
        ConstructionJob __instance,
        Accessible workplaceAccessible,
        ref (Inventory inventory, string goodId) __result)
    {
        if (!IsMaterialHaulingDisabled(__instance, workplaceAccessible))
        {
            return true;
        }

        __result = default;
        return false;
    }

    static bool IsMaterialHaulingDisabled(ConstructionJob job, Accessible workplaceAccessible)
    {
        var hubDisabler = workplaceAccessible.GetComponent<BuilderHubHaulingDisabler>();
        if (hubDisabler && hubDisabler.DisableHaulingMaterials)
        {
            return true;
        }

        var siteDisabler = job.GetComponent<ConstructionSiteBuilderHaulingSettings>();
        if (siteDisabler && siteDisabler.DisableBuilderHauling)
        {
            return true;
        }

        return false;
    }
}
