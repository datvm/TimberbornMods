using System.Reflection.Emit;

namespace ConfigurableBeaverWalk.Patches;

[HarmonyPatch]
public static class WorkingSpeedPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(Worker), nameof(Worker.WorkingSpeedMultiplier), MethodType.Setter)]
    public static void AddWorkSpeed(Worker __instance, ref float value)
    {
        var speed = MSettings.WorkSpeedMultiplier;

        if (MSettings.DifferentForBots && __instance.WorkerType == "Bot")
        {
            speed = MSettings.BotWorkSpeedMultiplier;
        }

        if (speed != 1)
        {
            value *= speed;
        }
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(WorkAtReservableExecutor), nameof(WorkAtReservableExecutor.Tick))]
    public static IEnumerable<CodeInstruction> Test(IEnumerable<CodeInstruction> instructions)
    {
        var expectingMethod = typeof(IDayNightCycle).PropertyGetter(nameof(IDayNightCycle.FixedDeltaTimeInHours));

        foreach (var ins in instructions)
        {
            yield return ins;

            if (ins.Calls(expectingMethod))
            {
                // Multiply it with worker speed
                yield return new CodeInstruction(OpCodes.Ldarg_0); // Load 'this' (WorkAtReservableExecutor instance)
                yield return CodeInstruction.Call(() => GetMultipliedDeltaTime);
            }
        }
    }

    static float? MinimumAmount;
    [HarmonyPostfix, HarmonyPatch(typeof(WorkAtReservableExecutor), nameof(WorkAtReservableExecutor.CalculateFinishTimestamp))]
    public static void EnsureOneTick(ref float __result, WorkAtReservableExecutor __instance)
    {
        var dnc = __instance._dayNightCycle;

        MinimumAmount ??= dnc.FixedDeltaTimeInHours * 2 / 24f;
        var min = dnc.PartialDayNumber + MinimumAmount.Value;
        if (__result < min)
        {
            __result = min;
        }
    }

    static float GetMultipliedDeltaTime(float fixedDeltaTimeHour, WorkAtReservableExecutor instance)
        => instance._worker.WorkingSpeedMultiplier * fixedDeltaTimeHour;
}
