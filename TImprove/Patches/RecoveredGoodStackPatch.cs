
namespace TImprove.Patches;

[HarmonyPatch(typeof(RecoveredGoodStack), nameof(RecoveredGoodStack.InitializeInventory))]
public static class RecoveredGoodStackPatch
{

    public static void Postfix(RecoveredGoodStack __instance)
    {
        if (Services.ModSettings.Instance?.PrioritizeRubbles != true) { return; }

        var prior = __instance.GetComponentFast<BuilderPrioritizable>();
        prior?.SetPriority(Timberborn.PrioritySystem.Priority.High);
    }

}
