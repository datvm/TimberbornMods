global using Timberborn.WorkSystem;
global using Timberborn.EnterableSystem;

namespace ConfigurableWorkplace.Patches;

[HarmonyPatch]
public static class WorkerPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(WorkplaceSpec), nameof(WorkplaceSpec.MaxWorkers), MethodType.Getter)]
    public static void MultiplyMaxWorkers(ref int __result)
    {
        var mul = MSettings.MaxWorkerMul;

        if (mul != 1f && __result > 0)
        {
            __result = Math.Max(1, Mathf.FloorToInt(__result * mul));
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(WorkplaceSpec), nameof(WorkplaceSpec.DisallowOtherWorkerTypes), MethodType.Getter)]
    public static bool AllowBots(ref bool __result)
    {
        if (MSettings.BotEverywhere)
        {
            __result = false;
            return false;
        }

        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Workplace), nameof(Workplace.ValidateEnterable))]
    public static void AllowEnterable(Workplace __instance)
    {
        if (MSettings.MaxWorkerMul != 1)
        {
            var enterable = __instance.GetComponentFast<EnterableSpec>();
            if (!enterable || !enterable.LimitedCapacityFinished) { return; }

            enterable._capacityFinished = __instance._workplaceSpec.MaxWorkers;
        }
    }

}
