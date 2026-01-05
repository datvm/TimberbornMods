namespace ModdableRecipes.Components;

public readonly record struct RecipeDropdownItemUIInfo(RecipeSpec? Spec, string DisplayName, Sprite? Icon, ModdableRecipeLockStatus Status);

public class ModdableManufactoryDropdownProvider(ModdableRecipeUIController controller) : BaseComponent, IAwakableComponent, IExtendedDropdownProvider, IDropdownProvider
{

#nullable disable
    ManufactoryDropdownProvider provider;
#nullable enable

    readonly List<RecipeDropdownItemUIInfo> items = [];
    public IReadOnlyList<string> Items { get; private set; } = [];

    public void Awake()
    {
        provider = GetComponent<ManufactoryDropdownProvider>();
    }

    public string FormatDisplayText(string value) => GetItem(value).DisplayName;
    public Sprite? GetIcon(string value) => GetItem(value).Icon;

    public string GetValue()
    {
        var recipeId = provider._manufactory.CurrentRecipe?.Id;
        var index = recipeId is null
            ? 0
            : items.FindIndex(r => r.Spec?.Id == recipeId);

        return index.ToString();
    }

    public void SetValue(string value)
    {
        var item = GetItem(value);
        if (item.Status != ModdableRecipeLockStatus.Unlocked
            && controller.OnLockedRecipeSelected(item)) // Recheck again and show UI
        {
            // Don't do anything
        }
        else
        {
            provider.SetValue(item.Spec?.DisplayLocKey);
        }
    }

    RecipeDropdownItemUIInfo GetItem(string? value) => items[int.TryParse(value, out var r) ? r : 0];

    public void UpdateItems()
    {
        // Rebuild original list as well
        provider.InitializeEntity();

        items.Clear();
        items.Add(controller.None);
        foreach (var r in provider._manufactory.ProductionRecipes)
        {
            var info = controller.GetRecipeInfo(r);
            if (info is not null)
            {
                items.Add(info.Value);
            }
        }

        Items = [.. items.Select((_, i) => i.ToString())];
    }



}
