namespace TImprove4Ui.Patches;

[HarmonyPatch(typeof(TopBarCounterRow))]
public static class MaterialCounterPatches
{

    public static void AddCounterEvents(TopBarCounterRow __instance)
    {
        var root = __instance._root;
        
        root.RegisterCallback<MouseEnterEvent>(_ =>
        {
            MaterialFinderService.Instance?.OnCounterHover(__instance);
        });

        root.RegisterCallback<MouseLeaveEvent>(_ =>
        {
            MaterialFinderService.Instance?.OnCounterLeft(__instance);
        });

        root.RegisterCallback<ClickEvent>(_ =>
        {
            MaterialFinderService.Instance?.OnCounterClicked(__instance);
        });
    }

    [HarmonyPostfix, HarmonyPatch(nameof(TopBarCounterRow.UpdateAndGetStock))]
    public static void CheckForVisibility(TopBarCounterRow __instance)
    {
        if (!MSettings.NeverHideCounter) { return; }

        var service = MaterialCounterService.Instance;
        if (service is null) { return; }

        var id = __instance._goodId;
        var visible = __instance._root.IsDisplayed();

        if (visible) // It has been visible before, it should stay that way
        {
            service.AddProducedGood(id);
        }
        else if (service.HasProducedGood(id))
        {
            __instance._root.ToggleDisplayStyle(true);
        }
    }

}
