
namespace ModdableWeathers.Patches.Game;

[HarmonyPatch(typeof(Sun))]
public static class SunPatches
{

    [HarmonyTranspiler, HarmonyPatch(nameof(Sun.UpdateColors))]
    public static IEnumerable<CodeInstruction> OverdriveFogSettings(IEnumerable<CodeInstruction> instructions)
    {
        var targetMethod = typeof(Sun).Method(nameof(Sun.GetFogSettings));
        var replacementMethod = typeof(SunPatches).Method(nameof(GetFogSettings));

        var counter = 1;

        foreach (var ins in instructions)
        {
            if (ins.Calls(targetMethod))
            {
                // Push the dayStageTransition argument back and call our replacement method
                yield return new(OpCodes.Ldarga_S, 1);
                yield return new(OpCodes.Ldc_I4, counter);
                counter--;
                yield return new(OpCodes.Call, replacementMethod);
            }
            else
            {
                yield return ins;
            }
        }

        if (counter != -1)
        {
            throw new InvalidOperationException("Failed to apply Sun.UpdateColors transpiler: Unmatched method calls.");
        }
    }

    static FogSettingsSpec GetFogSettings(string hazardId, DayStageColorsSpec _, in DayStageTransition dayStageTransition, bool from)
    {
        var spec = ModdableWeatherSpecService.Instance.GetFogSpecForWeather(hazardId);
        return spec[from ? dayStageTransition.CurrentDayStage : dayStageTransition.NextDayStage];
    }

}
