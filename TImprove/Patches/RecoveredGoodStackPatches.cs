namespace TImprove.Patches;

[HarmonyPatch(typeof(RecoveredGoodStack))]
public static class RecoveredGoodStackPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(RecoveredGoodStack.InitializeInventory))]
    public static void PrioritizeRubbles(RecoveredGoodStack __instance)
    {
        if (MSettings.Instance?.PrioritizeRubbles.Value != true) { return; }

        var prior = __instance.GetComponent<BuilderPrioritizable>();
        prior?.SetPriority(Timberborn.PrioritySystem.Priority.High);
    }

}
