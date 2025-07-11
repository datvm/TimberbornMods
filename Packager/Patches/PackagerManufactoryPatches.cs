
namespace Packager.Patches;

[HarmonyPatch(typeof(ManufactoryInventoryInitializer))]
public static class PackagerManufactoryPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(ManufactoryInventoryInitializer.Initialize))]
    public static void InitializeRecipes(ManufactoryInventoryInitializer __instance, Manufactory subject)
    {
        var spec = subject.GetComponentFast<PackagerManufactorySpec>();
        if (!spec) { return; }

        var list = spec._productionRecipeIds = [];
        var isPackager = spec.IsPackager;
        var goods = __instance._goodService;
        foreach (var good in PackagedGoodProvider.OriginalGoodIds)
        {
            if (!goods.HasGood(good) || !goods.HasGood(PackagedGoodProvider.GetPackagedGoodId(good))) { continue; }

            list.Add(PackagedGoodProvider.GetPackagerRecipe(good, isPackager));
        }
    }

}
