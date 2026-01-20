namespace ModdableRecipes.Patches;

[HarmonyPatch(typeof(ManufactoryDescriber))]
public static class ManufactoryDescriberPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(ManufactoryDescriber.DescribeEntity))]
    public static void RemoveLockedRecipes(ManufactoryDescriber __instance, ref IEnumerable<EntityDescription> __result)
    {
        var service = ModdableRecipeLockService.Instance
            ?? throw new InvalidOperationException($"{nameof(ModdableRecipeLockService)} is not initialized. This should not happen");

        var i = 0;
        var recipes = __instance._manufactory.ProductionRecipes;
        List<EntityDescription> result = [];
        foreach (var item in __result)
        {
            var id = recipes[i].Id;
            
            if (!service.IsLocked(id, out _))
            {
                result.Add(item);
            }

            ++i;
        }

        __result = result;
    }

}
