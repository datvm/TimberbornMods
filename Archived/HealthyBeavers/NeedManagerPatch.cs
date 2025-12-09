using Timberborn.Effects;
using Timberborn.NeedSystem;

namespace HealthyBeavers;

[HarmonyPatch(typeof(NeedManager))]
public static class NeedManagerPatch
{

    public const string BrokenTeethId = "BrokenTeeth";
    public const string BeeStingId = "BeeSting";
    public const string BadwaterContaminationId = "BadwaterContamination";
    public const string InjuryId = "Injury";

    [HarmonyPrefix, HarmonyPatch("ApplyEffect", [typeof(InstantEffect)], [ArgumentType.Ref])]
    public static bool ApplyInstantEffect(in InstantEffect effect) => ShouldApplyEffect(effect.NeedId);

    [HarmonyPrefix, HarmonyPatch("ApplyEffect", [typeof(ContinuousEffect), typeof(float)], [ArgumentType.Ref, ArgumentType.Normal])]
    public static bool ApplyContinousEffect(in ContinuousEffect effect) => ShouldApplyEffect(effect.NeedId);

    static bool ShouldApplyEffect(string id) => 
        id switch
        {
            BrokenTeethId => !ModSettings.BrokenTeeth,
            BeeStingId => !ModSettings.BeeSting,
            BadwaterContaminationId => !ModSettings.BadwaterContamination,
            InjuryId => !ModSettings.Injury,
            _ => true,
        };

}
