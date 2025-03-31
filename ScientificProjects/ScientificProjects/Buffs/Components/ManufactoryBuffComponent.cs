global using Timberborn.BlockSystem;

namespace ScientificProjects.Buffs;

public class ManufactoryBuffComponent : BaseComponent, IFinishedStateListener, IUnfinishedStateListener
{
    static readonly Dictionary<string, int> originalRecipesOutput = [];

    int? originalPowerInput;

    EntityComponent entity = null!;
    MechanicalNode? mechNode;
    MechanicalBuilding? mechBuilding;
    Manufactory? manufactory;
    MechanicalGraphManager graphMan = null!;
    BuffableComponent? buffable;

    public static readonly ImmutableHashSet<string> RecipeIds = ["MetalBlock", "TreatedPlank"];

    bool Deleted => !entity || entity.Deleted;

    public void Start()
    {
        buffable = this.GetBuffable();

        manufactory = GetComponentFast<Manufactory>();
        mechNode = GetComponentFast<MechanicalNode>();
        mechBuilding = GetComponentFast<MechanicalBuilding>();

        if (buffable is null) { return; }

        entity = GetComponentFast<EntityComponent>();

        buffable.OnBuffAdded += Buffable_OnBuffAdded;
        buffable.OnBuffRemoved += Buffable_OnBuffRemoved;
    }

    [Inject]
    public void Inject(MechanicalGraphManager graphMan)
    {
        this.graphMan = graphMan;
    }

    private void Buffable_OnBuffAdded(object sender, BuffInstance e) => ProcessBuffAddRemove(e, true);
    private void Buffable_OnBuffRemoved(object sender, BuffInstance e) => ProcessBuffAddRemove(e, false);

    void ProcessBuffAddRemove(BuffInstance e, bool add)
    {
        if (!e.Active || e is not FactionUpgradeBuffInst factionUpg || Deleted) { return; }

        ChangePowerUsage(add ? factionUpg.LessPowerBuffEffect.Value : 0);
        IncreaseRecipeAmount(1 + (add ? factionUpg.OutputBuffEffect.Value : 0));
    }

    void ChangePowerUsage(float delta)
    {
        if (!mechNode || !mechBuilding || graphMan is null || mechNode.Graph is null)
        {
            Debug.LogWarning("Cannot find any MechanicalNode or MechanicalBuilding for reducing power.");
            return;
        }

        if (!mechNode.enabled) { return; }

        graphMan.RemoveNode(mechNode);
        originalPowerInput ??= mechNode._nominalPowerInput;
        mechNode._nominalPowerInput = Math.Max(1, (int)MathF.Ceiling(
            originalPowerInput.Value * (enabled ? delta : 1)));

        graphMan.AddNode(mechNode);
    }

    static readonly MethodInfo recipeProductsField = typeof(RecipeSpec)
        .GetProperty("Products", BindingFlags.Public | BindingFlags.Instance)
        .SetMethod;
    void IncreaseRecipeAmount(float multiplier)
    {
        if (!manufactory)
        {
            Debug.LogWarning("Cannot find any Manufactory for increasing output.");
            return;
        }

        var recipe = manufactory.ProductionRecipes
            .FirstOrDefault(q => RecipeIds.Contains(q.Id));
        if (recipe is null)
        {
            Debug.LogWarning("Cannot find any applicable recipe for increasing output.");
            return;
        }

        var originalProduction = originalRecipesOutput.GetOrAdd(recipe.Id, () => recipe.Products[0].Amount);

        var newRecipes = recipe.Products
            .Select(q => new GoodAmountSpecNew()
            {
                Id = q.Id,
                Amount = (int)(originalProduction * multiplier),
            })
            .ToImmutableArray();

        recipeProductsField.Invoke(recipe, [newRecipes]);

        manufactory.SetRecipe(recipe);
    }

    void RefreshPowerBuff()
    {
        if (buffable?.Buffs.FirstOrDefault(q => q is FactionUpgradeBuffInst)
            is not FactionUpgradeBuffInst instant) { return; }

        ChangePowerUsage(instant.LessPowerBuffEffect.Value);
    }

    public void OnEnterFinishedState()
    {
        RefreshPowerBuff();
    }

    public void OnExitFinishedState()
    {
        RefreshPowerBuff();
    }

    public void OnEnterUnfinishedState()
    {
        RefreshPowerBuff();
    }

    public void OnExitUnfinishedState()
    {
        RefreshPowerBuff();
    }
}
