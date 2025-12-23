using System.Reflection.Emit;

namespace ModdableWeathers.Patches.Game;

[HarmonyPatch(typeof(DayStageCycle))]
public static class DayStageCyclePatches
{

    [HarmonyTranspiler, HarmonyPatch(nameof(DayStageCycle.Transition), [typeof(DayStage), typeof(DayStage), typeof(float)])]
    public static IEnumerable<CodeInstruction> GetNextStage(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;

        // Perform the calculation until transitionProgress
        foreach (var ins in instructions)
        {
            yield return ins;
            if (ins.opcode == OpCodes.Stloc_1)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new Exception("Failed to find transitionProgress storage in DayStageCycle.Transition");
        }

        // Load the parameters: this, 1st and 2nd arguments and the transitionProgress
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Ldarg_1);
        yield return new CodeInstruction(OpCodes.Ldarg_2);
        yield return new CodeInstruction(OpCodes.Ldloc_1);
        // Call our method
        yield return new CodeInstruction(OpCodes.Call, typeof(DayStageCyclePatches).Method(nameof(GetDayStageTransition)));
    }

    public static DayStageTransition GetDayStageTransition(DayStageCycle cycle, DayStage currentDayStage, DayStage nextDayStage, float transitionProgress)
    {
        // Always give the weather ID
        var weather = (ModdableWeatherService)cycle._weatherService;

        var currId = weather.CurrentWeather.Id;
        var nextId = weather.TomorrowWeather.Id;

        return new(currentDayStage, currId, nextDayStage, nextId, transitionProgress);
    }

}
