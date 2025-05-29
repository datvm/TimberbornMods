namespace ModdableWeather.Patches;

[HarmonyPatch(typeof(Sun))]
public static class SunRedirectionPatches
{

    [HarmonyTranspiler, HarmonyPatch(nameof(Sun.UpdateColors))]
    public static IEnumerable<CodeInstruction> UpdateColorsTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var code = instructions.ToList();

        RemoveFogRelatedCode(code);

        return code;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(Sun.UpdateColors))]
    public static void UpdateSunFog(DayStageTransition dayStageTransition)
    {
        ModdableSun.Instance.SetFogColor(dayStageTransition);
    }

    static void RemoveFogRelatedCode(List<CodeInstruction> code)
    {
        // Remove all fogs related code
        var start = code.FindIndex(q =>
            q.opcode == OpCodes.Call
            && q.operand is MethodInfo m
            && m.Name == nameof(Sun.GetFogSettings)
        );
        if (start == -1)
        {
            throw new InvalidOperationException("Could not find start of fog settings call in Sun.UpdateColors transpiler.");
        }

        // Go back to the instruction of dayStageTransition
        start = code.FindLastIndex(start - 1, q =>
            q.opcode == OpCodes.Ldarga_S
            && (byte)q.operand == 1
        );
        if (start == -1)
        {
            throw new InvalidOperationException("Could not find dayStageTransition in Sun.UpdateColors transpiler.");
        }

        // The end: when fogDensity is called
        var end = code.FindIndex(start, q =>
            q.opcode == OpCodes.Call
            && q.operand is MethodInfo m
            && m.Name == "set_fogDensity"
        );
        if (end == -1)
        {
            throw new InvalidOperationException("Could not find end of fog settings call in Sun.UpdateColors transpiler.");
        }

        code.RemoveRange(start, end - start + 1);
    }

}
