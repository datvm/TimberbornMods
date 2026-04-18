namespace TImprove.Patches;

[HarmonyPatch(typeof(RecoveredGoodStack))]
public static class RecoveredGoodStackPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(RecoveredGoodStack.InitializeInventory))]
    public static void PrioritizeRubbles(RecoveredGoodStack __instance)
    {
        if (MSettings.Instance is null) { return; }

        var prior = __instance.GetComponent<BuilderPrioritizable>();
        prior?.SetPriority(MSettings.Instance.DefaultRubblePriority);
    }

}
