namespace TImprove.Patches;

[HarmonyPatch(typeof(DevModeKeyBindingBlocker))]
public static class DevModeKeyBindingBlockerPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(DevModeKeyBindingBlocker.IsKeyBlocked))]
    public static void UnblockKeys(KeyBinding keyBinding, ref bool __result)
    {
        if (!__result) { return; }

        var s = MSettings.Instance;
        if (s is null) { return; }

        if (
            (keyBinding.Id == SpeedControlPanel.SlowDevGameSpeedKey && s.EnableSpeedS25.Value)
            || (keyBinding.Id == SpeedControlPanel.FastDevGameSpeedKey && s.EnableSpeed4.Value)
            || (keyBinding.Id == SpeedControlPanel.SuperFastDevGameSpeedKey && s.EnableSpeed5.Value)
        )
        {
            __result = false;
        }
    }

}
