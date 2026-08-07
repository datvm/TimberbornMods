namespace ModdableTimberborn.HaulingSystem.Patches;

[HarmonyPatch, HarmonyPatchCategory(ExtraHaulerTargetConfig.PatchCategoryName)]
public static class DistrictHaulCandidatesPatches
{
    /// <summary>
    /// Reimplements <see cref="DistrictHaulCandidates.GetWorkplaceBehaviorsOrdered"/> and merges
    /// extra hauler targets into the same weighted sort as vanilla haul candidates.
    /// </summary>
    [HarmonyPrefix, HarmonyPatch(typeof(DistrictHaulCandidates), nameof(DistrictHaulCandidates.GetWorkplaceBehaviorsOrdered))]
    public static bool AddExtraTargets(
        DistrictHaulCandidates __instance,
        IList<WorkplaceBehavior> workplaceBehaviors)
    {
        List<WeightedBehavior> weighted = [];

        foreach (var haulCandidate in __instance._haulCandidates)
        {
            haulCandidate.GetWeightedBehaviors(weighted);
        }

        var district = __instance.GetComponent<DistrictCenter>();
        var service = ExtraHaulerTargetService.Instance;
        if (district && service is not null)
        {
            service.AppendWeightedBehaviors(district, weighted);
        }

        weighted.Sort(static (a, b) => b.Weight.CompareTo(a.Weight));

        foreach (var item in weighted)
        {
            workplaceBehaviors.Add(item.WorkplaceBehavior);
        }

        return false;
    }

}
