namespace GlobalInventory.Components;

[AddTemplateModule2(typeof(GlobalInventoryManufactorySpec))]
public class GlobalInventoryManufactory(GlobalInventoryManufactoryService service)
    : BaseComponent, IAwakableComponent, IManufactoryLimiter, IEntityDescriber, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(GlobalInventoryManufactory));
    static readonly PropertyKey<bool> IngredientsTakenKey = new("GlobalIngredientsTaken");

    Manufactory manufactory = null!;
    BlockObject blockObject = null!;
    bool ingredientsTaken;

    public void Awake()
    {
        manufactory = GetComponent<Manufactory>();
        blockObject = GetComponent<BlockObject>();
        manufactory.ProductionProgressed += OnProductionProgressed;
        manufactory.ProductionFinished += OnProductionFinished;
        manufactory.RecipeChanged += OnRecipeChanged;
    }

    public float ProductionEfficiency()
        => service.ProductionEfficiency(manufactory.CurrentRecipe, ingredientsTaken);

    public float MaxProductionProgressChange(float expectedProductionProgressChange)
        => ProductionEfficiency() > 0f ? expectedProductionProgressChange : 0f;

    public IEnumerable<EntityDescription> DescribeEntity()
    {
        if (!blockObject.IsPreview)
        {
            yield break;
        }

        foreach (var description in service.Describe(manufactory))
        {
            yield return description;
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!ingredientsTaken)
        {
            return;
        }

        entitySaver.GetComponent(SaveKey).Set(IngredientsTakenKey, true);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        ingredientsTaken = s.Has(IngredientsTakenKey) && s.Get(IngredientsTakenKey);
    }

    void OnProductionProgressed(object sender, ProductionProgressedEventArgs e)
    {
        if (ingredientsTaken || !manufactory._ingredientsConsumed)
        {
            return;
        }

        if (!service.TryGetRecipe(manufactory.CurrentRecipe, out var spec))
        {
            return;
        }

        service.TakeIngredients(spec);
        ingredientsTaken = true;
    }

    void OnProductionFinished(object sender, EventArgs e)
    {
        if (service.TryGetRecipe(manufactory.CurrentRecipe, out var spec))
        {
            service.GiveProducts(spec);
        }

        ingredientsTaken = false;
    }

    void OnRecipeChanged(object sender, EventArgs e)
    {
        ingredientsTaken = false;
    }
}
