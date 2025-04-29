namespace TImprove4Ui.Patches;

[HarmonyPatch]
public static class MaterialCounterPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(TopBarCounterRow), nameof(TopBarCounterRow.UpdateAndGetStock))]
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
