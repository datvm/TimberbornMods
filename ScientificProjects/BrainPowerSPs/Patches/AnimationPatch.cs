using Timberborn.TimbermeshAnimations;

namespace BrainPowerSPs.Patches;


[HarmonyPatch]
public static class AnimationPatch
{

    [HarmonyPrefix, HarmonyPatch(typeof(TimbermeshAnimatorController), nameof(TimbermeshAnimatorController.SetBool))]
    public static void FixBotWheelAnimationBool(ref string parameterName, TimbermeshAnimatorController __instance)
    {
        if ((parameterName == "WalkingWork" || parameterName == "PushingWork") && !__instance._boolValues.ContainsKey(parameterName))
        {
            parameterName = "Walking";
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(TimbermeshAnimatorController), nameof(TimbermeshAnimatorController.SetFloat))]
    public static void FixBotWheelAnimationFloat(ref string parameterName, TimbermeshAnimatorController __instance)
    {
        if (parameterName == "WorkshopSpeed" && !__instance._boolValues.ContainsKey(parameterName))
        {
            parameterName = "WalkingSpeed";
        }
    }

}
