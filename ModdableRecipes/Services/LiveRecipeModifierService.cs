namespace ModdableRecipes.Services;

public class LiveRecipeModifierService(
    RecipeSpecService specs,
    EventBus eb,
    EntityRegistry entityRegistry,
    StatusListFragment statusListFragment
)
{
    static readonly ImmutableArray<PropertyInfo> Properties = [..typeof(RecipeSpec)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .Where(p => p.CanWrite && p.Name != nameof(RecipeSpec.Id))
    ];

    public void Modify(string id, Func<RecipeSpec, RecipeSpec> modifier)
    {
        var recipe = specs.GetRecipe(id);
        var old = recipe with { };

        var modified = modifier(recipe);

        foreach (var p in Properties)
        {
            p.SetValue(recipe, p.GetValue(modified));
        }

        UpdateManufactories(recipe);
        statusListFragment.UpdateFragment();

        eb.Post(new RecipeModifiedEvent(id, old, recipe));
    }

    void UpdateManufactories(RecipeSpec recipe)
    {
        var id = recipe.Id;
        foreach (var e in entityRegistry.Entities)
        {
            var man = e.GetComponent<Manufactory>();
            if (!man) { continue; }

            if (man.CurrentRecipe?.Id == id)
            {
                man.SetRecipe(recipe);
            }
        }
    }

}
