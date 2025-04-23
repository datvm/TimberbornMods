global using Timberborn.TimbermeshAnimations;

namespace ConfigurableWorkplace.Patches;

[HarmonyPatch]
public static class BotAnimationPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(TimbermeshAnimatorController), nameof(TimbermeshAnimatorController.SetBool))]
    public static void FixBotWheelAnimationBool(ref string parameterName, TimbermeshAnimatorController __instance)
    {
        if (!__instance._boolValues.ContainsKey(parameterName))
        {
            parameterName = "Walking";
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(TimbermeshAnimatorController), nameof(TimbermeshAnimatorController.SetFloat))]
    public static void FixBotWheelAnimationFloat(ref string parameterName, TimbermeshAnimatorController __instance)
    {
        if (!__instance._boolValues.ContainsKey(parameterName))
        {
            parameterName = "WalkingSpeed";
        }
    }

}
