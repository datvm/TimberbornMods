namespace ModdableToolGroupsHotkeys.Patches;

[HarmonyPatch(typeof(KeyBinding))]
public static class KeybindingPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(KeyBinding.UpdatePressedState))]
    public static void OnKeyStateUpdated(KeyBinding __instance)
    {
        if (__instance.IsDown)
        {
            KeyBindingEventService.Instance?.RaiseOnDown(__instance);
        }
    }

}
