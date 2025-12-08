namespace ConfigurableToolGroups.Patches;

[HarmonyPatch(typeof(ToolGroupButton))]
public static class ButtonsPatches
{
    [HarmonyPrefix, HarmonyPatch(nameof(ToolGroupButton.IsVisible), MethodType.Getter)]
    public static bool UpdateVisibility(ToolGroupButton __instance, ref bool __result)
    {
        var info = ModdableToolGroupButtonService.Instance[__instance];
        if (info is null) { return true; }

        foreach (var grp in info.Children)
        {
            if (grp.IsVisible)
            {
                __result = true;
                return false;
            }
        }

        return true;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(ToolGroupButton.OnToolGroupEntered))]
    public static void ActivateParents(ToolGroupButton __instance, ToolGroupEnteredEvent toolGroupOpenedEvent)
    {
        if (toolGroupOpenedEvent.ToolGroup != __instance._toolGroup) { return; }

        var level = 0;

        var parent = ModdableToolGroupButtonService.Instance[__instance]?.Parent;
        if (parent is null) { goto SET_TOP_BEFORE_RETURN; }

        while (parent is not null)
        {
            level++;
            var btn = parent.Button;

            btn.ToolButtonsElement.ToggleDisplayStyle(true);
            btn._toolGroupButtonWrapper.AddToClassList(ToolGroupButton.ActiveClassName);

            parent = parent.Parent;
        }

    SET_TOP_BEFORE_RETURN:
        ToolPanelPositioningService.Instance?.SetTop(level);
    }

    [HarmonyPrefix, HarmonyPatch(nameof(ToolGroupButton.OnToolGroupExited))]
    public static void DeactivateParents(ToolGroupButton __instance, ToolGroupExitedEvent toolGroupExitedEvent)
    {
        if (toolGroupExitedEvent.ToolGroup != __instance._toolGroup) { return; }

        var parent = ModdableToolGroupButtonService.Instance[__instance]?.Parent;
        if (parent is null) { return; }

        while (parent is not null)
        {
            var btn = parent.Button;

            btn.ToolButtonsElement.ToggleDisplayStyle(false);
            btn._toolGroupButtonWrapper.RemoveFromClassList(ToolGroupButton.ActiveClassName);

            parent = parent.Parent;
        }
    }

}
