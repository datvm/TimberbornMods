using System.Reflection.Emit;

namespace MoreBuildingRenovations.Patches;

[HarmonyPatch]
public static class FarmingSpeedPatches
{

    [HarmonyTranspiler, HarmonyPatch(typeof(RemoveYieldExecutor), nameof(RemoveYieldExecutor.Remove))]
    /* IL code:
		// Launch(_yielderRemover.ReservedYielder.RemovalTimeInHours);
		IL_000d: ldarg.0
		IL_000e: ldarg.0
		IL_000f: ldfld class Timberborn.Yielding.YielderRemover Timberborn.Yielding.RemoveYieldExecutor::_yielderRemover
		IL_0014: callvirt instance class Timberborn.Yielding.Yielder Timberborn.Yielding.YielderRemover::get_ReservedYielder()
		IL_0019: callvirt instance float32 Timberborn.Yielding.Yielder::get_RemovalTimeInHours()
		IL_001e: call instance void [Timberborn.ReservableSystem]Timberborn.ReservableSystem.WorkAtReservableExecutor::Launch(float32)
		// return true;
		IL_0023: ldc.i4.1
		IL_0024: ret
     */
    public static IEnumerable<CodeInstruction> PatchHarvestSpeed(IEnumerable<CodeInstruction> instructions)
    {
        var launchMethod = typeof(WorkAtReservableExecutor).Method(nameof(WorkAtReservableExecutor.Launch));

        foreach (var ins in instructions)
        {
            if (ins.Calls(launchMethod))
            {
                // stack: this, hours → this, hours, this → this, adjustedHours
                yield return new(OpCodes.Ldarg_0);
                yield return new(OpCodes.Call, typeof(FarmingSpeedPatches).Method(nameof(GetMultipliedHarvestTime)));
            }

            yield return ins;
        }
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(PlantExecutor), nameof(PlantExecutor.Launch))]
    /* IL code:
	    // float hours = ...PlantTimeInHours / _worker.WorkingSpeedMultiplier;
	    ...
	    IL_0058: callvirt instance float32 Worker::get_WorkingSpeedMultiplier()
	    IL_005d: div
	    IL_005e: stloc.0
     */
    public static IEnumerable<CodeInstruction> PatchPlantSpeed(IEnumerable<CodeInstruction> instructions)
    {
        var workingSpeed = typeof(Worker).PropertyGetter(nameof(Worker.WorkingSpeedMultiplier));
        var afterWorkingSpeedDiv = false;

        foreach (var ins in instructions)
        {
            if (ins.Calls(workingSpeed))
            {
                afterWorkingSpeedDiv = true;
                yield return ins;
                continue;
            }

            yield return ins;

            // After: hours = plantTime / WorkingSpeedMultiplier
            // Inject: hours = GetMultipliedPlantingTime(hours, this)
            if (afterWorkingSpeedDiv && ins.opcode == OpCodes.Div)
            {
                yield return new(OpCodes.Ldarg_0);
                yield return new(OpCodes.Call, typeof(FarmingSpeedPatches).Method(nameof(GetMultipliedPlantingTime)));
            }

            afterWorkingSpeedDiv = false;
        }
    }

    static float GetMultipliedHarvestTime(float value, RemoveYieldExecutor instance)
        => GetMultipliedTime(instance, value, harvest: true);

    static float GetMultipliedPlantingTime(float value, PlantExecutor instance)
        => GetMultipliedTime(instance, value, harvest: false);

    static float GetMultipliedTime(BaseComponent instance, float value, bool harvest)
    {
        var workplace = instance.GetComponent<Worker>()?.Workplace;
        if (!workplace) { return value; }

        var comp = workplace!.GetComponent<FarmhouseActionSpeed>();
        if (!comp) { return value; }

        return value / (harvest ? comp.HarvestSpeedMultiplier : comp.PlantingSpeedMultiplier);
    }

}
