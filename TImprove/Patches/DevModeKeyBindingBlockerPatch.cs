namespace TImprove.Patches;

[HarmonyPatch("Timberborn.Debugging.DevModeKeyBindingBlocker", "IsKeyBlocked")]
public static class DevModeKeyBindingBlockerPatch
{

    #region Speed Control

    static T GetSpeedControlPanelField<T>(string name, T fallback)
    {
        return typeof(SpeedControlPanel).GetField(name, BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null)
            is T v
            ? v
            : fallback;
    }

    static readonly string SlowDevGameSpeedKey = GetSpeedControlPanelField(nameof(SlowDevGameSpeedKey), nameof(SlowDevGameSpeedKey));
    static readonly string FastDevGameSpeedKey = GetSpeedControlPanelField(nameof(FastDevGameSpeedKey), nameof(FastDevGameSpeedKey));
    static readonly string SuperFastDevGameSpeedKey = GetSpeedControlPanelField(nameof(SuperFastDevGameSpeedKey), nameof(SuperFastDevGameSpeedKey));

    #endregion

    public static bool Prefix(KeyBinding keyBinding, ref bool __result)
    {
        var s = Services.ModSettings.Instance;
        if (s is null) { return true; }

        if (
            (s.EnableSpeedS25 && keyBinding.Id == SlowDevGameSpeedKey)
            || (s.EnableSpeed4 && keyBinding.Id == FastDevGameSpeedKey)
            || (s.EnableSpeed5 && keyBinding.Id == SuperFastDevGameSpeedKey)
        )
        {
            __result = false;
            return false;
        }

        return true;
    }

}
