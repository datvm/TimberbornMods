namespace MacroManagement.Components.DummyComponents;

public class DummyManufactory : Manufactory, IWarningDummyComponent<DummyManufactory, Manufactory>
{
#nullable disable
    public MMComponent MMComponent { get; set; }
    public ILoc T { get; set; }
    public DialogBoxShower DiaglogBoxShower { get; set; }
#nullable enable

    public void Init(Manufactory original)
    {
        Inventory = original.Inventory;
        ProductionRecipes = original.ProductionRecipes;
        CurrentRecipe = original.CurrentRecipe;
    }

    [Inject]
    public void Inject(ILoc t, DialogBoxShower diag) => this.InjectWarningDummy(t, diag);

    public new void SetRecipe(RecipeSpec selectedRecipe) =>
        this.Confirm("LV.MacM.ManufactoryWarning", () =>
        {
            this.Proxy(q => q.SetRecipe(selectedRecipe));
            CurrentRecipe = selectedRecipe;
        });

}

[HarmonyPatch(typeof(Manufactory))]
public static class ManufactoryRedirect
{
    [HarmonyPrefix, HarmonyPatch(nameof(Manufactory.Awake))]
    public static bool BypassAwake(Manufactory __instance)
        => __instance.PatchBypass<DummyManufactory, Manufactory>();

    [HarmonyPrefix, HarmonyPatch(nameof(Manufactory.SetRecipe))]
    public static bool RedirectSetRecipe(Manufactory __instance, RecipeSpec selectedRecipe)
        => __instance.PatchRedirect<DummyManufactory, Manufactory>(q => q.SetRecipe(selectedRecipe));

}