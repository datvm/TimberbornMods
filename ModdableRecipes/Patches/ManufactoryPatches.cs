namespace ModdableRecipes.Patches;

[HarmonyPatch(typeof(Manufactory))]
public static class ManufactoryPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(Manufactory.Load))]
    public static void InitInventory(Manufactory __instance)
    {
        InitInventoryForRecipe(__instance, __instance.CurrentRecipe);
    }

    [HarmonyPrefix, HarmonyPatch(nameof(Manufactory.SetRecipe))]
    public static void InitInventoryForRecipe(Manufactory __instance, RecipeSpec? selectedRecipe)
    {
        var inv = __instance.Inventory;
        inv.Disable();

        var registry = __instance.Inventory._allowedGoods;
        registry._outputGoods.Clear();

        var goods = selectedRecipe is null 
            ? [] 
            : RecipeGoodsProcessorReference.Instance.Processor.ProcessRecipes([selectedRecipe.Id]);

        Dictionary<string, StorableGoodAmount> storables = [];
        foreach (var g in registry._storableGoods)
        {
            storables[g.StorableGood.GoodId] = g;
        }

        registry._inputGoods.Clear();
        registry._outputGoods.Clear();
        registry._storableGoods.Clear();

        foreach (var (g, amount) in goods)
        {
            var id = g.GoodId;

            registry.Add(new StorableGoodAmount(g, amount));
            storables.Remove(id);
        }

        foreach (var item in storables.Values)
        {
            registry.Add(new StorableGoodAmount(new(item.StorableGood.GoodId, true, false), item.Amount));
        }

        inv.Enable();
    }

}
