namespace MacroManagement.Patches;

[HarmonyPatch(typeof(EntitySelectionService))]
public class EntitySelectionServicePatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(EntitySelectionService.SelectSelectable))]
    public static bool TryAddSelection(ref SelectableObject target, EntitySelectionService __instance)
    {
        var service = MultiSelectService.Instance;
        if (service is null) { return true; }

        var modified = service.TryAddSelection(target);
        if (modified is null)
        {
            return false;
        }
        else
        {
            target = modified;
        }
        return true;
    }

}
