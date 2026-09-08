using Timberborn.WorkshopsUI;

namespace UnlockableRecipe;


[HarmonyPatch]
public static class RecipeLockerPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(ManufactoryDropdownProvider), nameof(ManufactoryDropdownProvider.GetIcon))]
    public static bool GetIconPatch(string value, ManufactoryDropdownProvider __instance, ref Sprite? __result)
    {
        var registry = RecipeLockerRegistry.Instance;
        var lockIcon = RecipeLockerUi.Instance?.LockIcon;
        if (registry is null || lockIcon is null) { return true; }

        var spec = __instance.GetRecipeSpec(value);
        if (spec is null) // The game code currently returns null so we just short-circuit it anyway
        {
            __result = null;
            return false;
        }

        if (registry.IsRecipeLocked(spec.Id))
        {
            __result = lockIcon;
            return false;
        }

        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ManufactoryDropdownProvider), nameof(ManufactoryDropdownProvider.SetValue))]
    public static bool SetValuePatch(string value, ManufactoryDropdownProvider __instance)
    {
        var registry = RecipeLockerRegistry.Instance;
        if (registry is null) { return true; }
        var spec = __instance.GetRecipeSpec(value);
        if (spec is null) { return true; }

        var reason = registry.IsRecipeBlocked(spec.Id, registry.T);
        if (reason is not null)
        {
            RecipeLockerUi.Instance?.ShowLockedRecipeError(reason);
            return false;
        }

        return true;
    }

}
