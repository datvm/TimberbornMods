namespace XtraPlzNoCrashes.Patches;

[HarmonyPatch]
public static class CharacterAnimatorPatches
{
    const string FallbackAnimation = "ForcedIdle";

    [HarmonyPrefix, HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.SetActiveAnimation))]
    public static bool UseExistingAnimation(ref string animationName, CharacterModel __instance)
    {
        if (__instance._characterAnimator.HasParameter(animationName))
        {
            return true;
        }

        if (__instance._characterAnimator.HasParameter(FallbackAnimation))
        {
            animationName = FallbackAnimation;
            return true;
        }

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CharacterAnimator), nameof(CharacterAnimator.SetBool))]
    public static bool SkipMissingBool(CharacterAnimator __instance, string parameterName) =>
        __instance.HasParameter(parameterName);

    [HarmonyPrefix, HarmonyPatch(typeof(CharacterAnimator), nameof(CharacterAnimator.SetFloat))]
    public static bool SkipMissingFloat(CharacterAnimator __instance, string parameterName) =>
        __instance.HasParameter(parameterName);

}
