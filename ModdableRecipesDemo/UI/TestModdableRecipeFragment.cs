namespace ModdableRecipesDemo.UI;

public class TestModdableRecipeFragment(
    ModdableRecipeLockService locker,
    ILoc t,
    DialogService diag,
    RecipeSpecService specs,
    LiveRecipeModifierService liveRecipeModifier
) : BaseEntityPanelFragment<Manufactory>
{
    const string CustomPlankId = "CustomPlank";
    static readonly ImmutableArray<string> CustomRecipes = ["Plank", "DoublePlank", "TriplePlank", CustomPlankId];

    VisualElement list = null!;
    protected override void InitializePanel()
    {
        list = panel.AddChild().SetPadding(5);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component) { return; }

        var recipes = component!.ProductionRecipes;
        var isLumberMill = recipes.Any(r => CustomRecipes.Contains(r.Id));
        if (!isLumberMill)
        {
            ClearFragment();
            return;
        }

        RefreshList();
    }

    void RefreshList()
    {
        list.Clear();

        foreach (var r in CustomRecipes)
        {
            var locked = locker.IsLocked(r, out _);

            list.AddGameButtonPadded($"{(locked ? "Unlock" : "Lock")} {r}", onClick: () => ToggleLock(r));
        }

        list.AddGameButtonPadded("Change custom recipe output", onClick: ChangeCustomOutput);
    }

    async void ChangeCustomOutput()
    {
        var curr = specs.GetRecipe(CustomPlankId);

        var id = await diag.PromptAsync("Enter output good:", curr.Products[0].Id);
        if (string.IsNullOrEmpty(id)) { return; }

        var amountStr = await diag.PromptAsync("Enter output amount:", curr.Products[0].Amount.ToString());
        if (string.IsNullOrEmpty(amountStr) || !int.TryParse(amountStr, out var o) || o <= 0) { return; }

        liveRecipeModifier.Modify(CustomPlankId, r => r with
        {
            Products = [new() {
                Id = id,
                Amount = o,
            }],
        });
    }

    void ToggleLock(string id)
    {
        var isLocked = locker.IsLocked(id, out _);

        if (isLocked)
        {
            locker.Unlock(id);
        }
        else
        {
            locker.Lock(id, t.T("LV.MRecDemo.LockDesc"));
        }
    }

    public override void ClearFragment()
    {
        base.ClearFragment();
        list.Clear();
    }

}
