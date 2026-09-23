namespace ScienceShop.UI;

[BindFragment]
public class ScienceShopQueueFragment(
    ILoc t,
    VisualElementLoader veLoader,
    VisualElementInitializer veInit,
    DropdownItemsSetter dropdownItems,
    DialogService diag
) : BaseEntityPanelFragment<ConsumeScienceManufactory>
{
    Image productionIcon = null!;
    Label production = null!;
    Dropdown recipeDropdown = null!;
    VisualElement recipeContent = null!;
    string shownRecipeId = "";
    QueueRecipeDropdownProvider recipes = null!;
    Toggle indefinite = null!;
    VisualElement quantityRow = null!;
    Label quantity = null!;
    Button plus = null!;
    Button minus = null!;
    Button clear = null!;
    bool refreshing;

    protected override void InitializePanel()
    {
        panel.AddGameLabel(t.T("LV.ScS.CurrentProduction").Bold()).SetMarginBottom(5);

        var productionRow = panel.AddRow().AlignItems().SetMarginBottom();
        productionIcon = productionRow.AddImage().SetSize(40).SetMarginRight(5);
        production = productionRow.AddGameLabel();

        panel.AddGameLabel(t.T("LV.ScS.CurrentQueue").Bold()).SetMarginBottom(5);

        recipeDropdown = panel.AddDropdown();
        recipeDropdown.Initialize(veInit);
        recipes = new(t);
        recipes.Changed += OnRecipeSelected;
        recipeContent = panel.AddChild().SetMarginBottom();

        indefinite = panel.AddGamePanelToggle(t.T("LV.ScS.Indefinite"), OnIndefiniteChanged)
            .SetMarginBottom(5);

        quantityRow = panel.AddRow().AlignItems();
        quantityRow.AddGameLabel(t.T("LV.ScS.Quantity")).SetMarginRight(5);
        quantityRow.AddChild().SetMarginLeftAuto();
        quantity = quantityRow.AddGameLabel().SetMarginRight(10);
        plus = quantityRow.AddPlusButton().AddAction(OnPlus).SetMarginRight(5);
        minus = quantityRow.AddMinusButton().AddAction(OnMinus).SetMarginRight(5);
        clear = quantityRow.AddGameButtonPadded(t.T("LV.ScS.ClearQueue"), OnClearQueue);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is null)
        {
            return;
        }

        BindRecipes();
        Refresh();
    }

    public override void ClearFragment()
    {
        shownRecipeId = "";
        recipeContent.Clear();
        base.ClearFragment();
    }

    public override void UpdateFragment() => Refresh();

    void BindRecipes()
    {
        var items = component!.Manufactory.ProductionRecipes
            .OrderBy(r => component.Service.GetOrder(r))
            .ThenBy(r => t.T(r.DisplayLocKey), StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        recipes.SetRecipes(items);
        dropdownItems.SetItems(recipeDropdown, recipes);

        var selected = component.QueueRecipe ?? items.FirstOrDefault();
        if (selected is not null)
        {
            recipes.SelectedId = selected.Id;
            recipeDropdown.UpdateSelectedValue();
        }
    }

    void Refresh()
    {
        if (component is null)
        {
            return;
        }

        refreshing = true;
        SetProduction(component.CurrentRecipe);
        SetSelectedRecipe(recipes.SelectedRecipe);

        indefinite.SetValueWithoutNotify(component.Indefinite);
        quantityRow.SetDisplay(!component.Indefinite);
        quantity.text = component.HasQueue ? component.Remaining.ToString() : "0";

        var hasSelection = recipes.SelectedRecipe is not null;
        plus.enabledSelf = hasSelection;
        minus.enabledSelf = component.HasQueue && component.Remaining > 0;
        clear.enabledSelf = component.HasQueue;
        refreshing = false;
    }

    void SetProduction(RecipeSpec? recipe)
    {
        if (recipe is null)
        {
            productionIcon.SetDisplay(false);
            production.text = t.T("LV.ScS.None");
            return;
        }

        productionIcon.SetDisplay(true);
        var sprite = RecipeIcon(recipe);
        if (sprite is not null)
        {
            productionIcon.sprite = sprite;
        }

        production.text = t.T(recipe.DisplayLocKey);
    }

    void SetSelectedRecipe(RecipeSpec? recipe)
    {
        var id = recipe?.Id ?? "";
        if (id == shownRecipeId)
        {
            return;
        }

        shownRecipeId = id;
        recipeContent.Clear();
        if (recipe is null || component is null)
        {
            return;
        }

        var describer = component.GetComponent<ManufactoryDescriber>();
        if (!describer)
        {
            shownRecipeId = "";
            return;
        }

        var item = veLoader.LoadVisualElement("Game/EntityPanel/ProductionProgressFragment");
        item.RemoveFromClassList("entity-sub-panel");
        item.RemoveFromClassList("bg-sub-box--green");

        var (input, output) = describer.DescribeRecipe(recipe);
        item.Q("InputWrapper").Add(input);
        item.Q("OutputWrapper").Add(output);

        var row = item.Q("ProductionItem");
        row.style.marginTop = 0;
        row.style.paddingBottom = 0;
        item.Q(className: "production-progress-fragment__progress-wrapper").style.top = 0;

        var workplace = component.GetComponent<Workplace>();
        var workers = workplace && workplace.MaxWorkers > 0 ? workplace.MaxWorkers : 1;
        item.Q<Label>("ProgressText").text = describer.GetCraftingTime(recipe, workers);
        item.Q("CraftingTime").SetDisplay(false);
        item.Q("FuelRemaining").SetDisplay(false);
        recipeContent.Add(item);
    }

    async void OnRecipeSelected(string id)
    {
        if (refreshing || component is null)
        {
            return;
        }

        var recipe = recipes.SelectedRecipe;
        if (recipe is null)
        {
            return;
        }

        if (!component.WouldReplaceQueue(recipe))
        {
            return;
        }

        OnClearQueue();
        component.ReplaceQueueRecipe(recipe);
        Refresh();
    }

    async void OnPlus()
    {
        if (component is null || recipes.SelectedRecipe is not { } recipe)
        {
            return;
        }

        if (component.WouldReplaceQueue(recipe)
            && !await diag.ConfirmAsync("LV.ScS.QueueReplaceWarning", localized: true))
        {
            return;
        }

        component.Add(recipe, 1);
        Refresh();
    }

    void OnMinus()
    {
        component?.RemoveOne();
        Refresh();
    }

    async void OnIndefiniteChanged(bool value)
    {
        if (refreshing || component is null || recipes.SelectedRecipe is not { } recipe)
        {
            return;
        }

        if (value && component.WouldReplaceQueue(recipe)
            && !await diag.ConfirmAsync("LV.ScS.QueueReplaceWarning", localized: true))
        {
            refreshing = true;
            indefinite.SetValueWithoutNotify(false);
            refreshing = false;
            return;
        }

        component.SetIndefinite(recipe, value);
        Refresh();
    }

    void OnClearQueue()
    {
        component?.ClearQueue();
        Refresh();
    }

    static Sprite? RecipeIcon(RecipeSpec recipe) => recipe.Icon?.Asset ?? recipe.UIIcon?.Value;
}

class QueueRecipeDropdownProvider(ILoc t) : IExtendedDropdownProvider
{
    readonly List<RecipeSpec> recipes = [];
    readonly Dictionary<string, RecipeSpec> byId = [];

    public List<string> Ids { get; } = [];
    public string SelectedId { get; set; } = "";
    public event Action<string>? Changed;

    public RecipeSpec? SelectedRecipe => byId.GetValueOrDefault(SelectedId);

    public IReadOnlyList<string> Items => Ids;

    public void SetRecipes(IReadOnlyList<RecipeSpec> items)
    {
        recipes.Clear();
        byId.Clear();
        Ids.Clear();
        foreach (var recipe in items)
        {
            recipes.Add(recipe);
            byId[recipe.Id] = recipe;
            Ids.Add(recipe.Id);
        }
    }

    public string GetValue() => SelectedId;

    public void SetValue(string value)
    {
        if (SelectedId == value)
        {
            return;
        }

        SelectedId = value;
        Changed?.Invoke(value);
    }

    public string FormatDisplayText(string value, bool selected)
        => byId.TryGetValue(value, out var recipe) ? t.T(recipe.DisplayLocKey) : value;

    public Sprite GetIcon(string value)
    {
        if (byId.TryGetValue(value, out var recipe))
        {
            return recipe.Icon?.Asset ?? recipe.UIIcon?.Value!;
        }

        return null!;
    }

    public ImmutableArray<string> GetItemClasses(string value) => [];
}
