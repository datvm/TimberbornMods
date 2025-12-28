namespace ModdableWeathers.Patches.Game;

[HarmonyPatch(typeof(BadtideStreakAchievement))]
public static class BadtideStreakAchievementPatches
{

    [HarmonyTranspiler, HarmonyPatch(nameof(BadtideStreakAchievement.OnHazardousWeatherStarted))]
    public static IEnumerable<CodeInstruction> ChangeDetection(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;

        foreach (var ins in instructions)
        {
            if (ins.opcode == OpCodes.Isinst)
            {
                found = true;

                yield return new(OpCodes.Callvirt, typeof(IHazardousWeather).PropertyGetter(nameof(IHazardousWeather.Id)));
                yield return new(OpCodes.Ldstr, GameBadtideWeather.WeatherId);
                yield return new(OpCodes.Call, typeof(string).Method("op_Equality"));
            }
            else
            {
                yield return ins;
            }   
        }

        if (!found)
        {
            throw new InvalidOperationException("Failed to apply BadtideStreakAchievementPatches.ChangeDetection transpiler");
        }
    }

}
