namespace DisableHauling.Patches;

[HarmonyPatch(typeof(HaulCandidate))]
public static class HaulCandidatePatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(HaulCandidate.GetWeightedBehaviors))]
    public static bool DisableHauling(HaulCandidate __instance) =>
        __instance.GetComponent<DisableHaulingComponent>()?.DisableHauling != true;
}
