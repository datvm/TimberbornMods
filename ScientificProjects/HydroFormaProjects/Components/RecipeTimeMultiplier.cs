namespace HydroFormaProjects.Components;

public class RecipeTimeMultiplierSpec : BaseComponent
{
    [SerializeField]
    public string id = "";
    [SerializeField]
    public float multiplier = 1f;

    public string Id => id;
    public float Multiplier => multiplier;
}

public class RecipeTimeMultiplier : BaseComponent
{
    static readonly ConditionalWeakTable<RecipeSpec, RecipeTimeMultiplierApplier> appliers = [];

    public void Start()
    {
        var manufactory = GetComponentFast<Manufactory>();
        if (!manufactory)
        {
            throw new InvalidOperationException($"{nameof(RecipeTimeMultiplier)} is supposed to be added to objects with {nameof(Manufactory)} components only");
        }

        var spec = GetComponentFast<RecipeTimeMultiplierSpec>();

        foreach (var recipe in manufactory.ProductionRecipes)
        {
            if (!appliers.TryGetValue(recipe, out var applier))
            {
                applier = new RecipeTimeMultiplierApplier(recipe);
                appliers.Add(recipe, applier);
            }

            applier.ApplyMultiplier(spec.Id, spec.Multiplier);
        }
    }

}

public record RecipeTimeMultiplierApplier(RecipeSpec Recipe)
{

    readonly HashSet<string> appliedModifierIds = [];
    public float Multiplier { get; private set; } = 1f;
    public float OriginalTime { get; } = Recipe.CycleDurationInHours;

    static readonly MethodInfo CycleDurationInHoursSetter = typeof(RecipeSpec).PropertySetter(nameof(RecipeSpec.CycleDurationInHours))
        ?? throw new InvalidOperationException($"Cannot find setter for {nameof(RecipeSpec)}.{nameof(RecipeSpec.CycleDurationInHours)} property");
    public void ApplyMultiplier(string id, float multiplier)
    {
        if (appliedModifierIds.Contains(id)) { return; }

        appliedModifierIds.Add(id);
        Multiplier *= multiplier;
        
        CycleDurationInHoursSetter.Invoke(Recipe, [OriginalTime * Multiplier]);

        Debug.Log($"Set {Recipe.Id} cycle duration to {OriginalTime * Multiplier} (Applied Multiplier: {id} {multiplier}, Total Mul: {Multiplier})");
    }

}