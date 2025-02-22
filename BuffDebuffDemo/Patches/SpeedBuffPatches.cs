namespace BuffDebuffDemo.Patches;

[HarmonyPatch]
public static class SpeedBuffPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(WalkerSpeedManager), nameof(WalkerSpeedManager.GetWalkerBaseSpeed))]
    public static void PatchSpeedBuff(WalkerSpeedManager __instance, ref float __result)
    {
        var buffs = __instance.GetBuffEffects<SpeedBuffEffect>();
        if (!buffs.Any()) { return; }

        var multiplier = 1f;
        foreach (var buff in buffs)
        {
            multiplier *= buff.Value;
        }

        __result *= multiplier;
    }

}
