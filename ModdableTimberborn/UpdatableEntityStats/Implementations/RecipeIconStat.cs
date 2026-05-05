namespace ModdableTimberborn.UpdatableEntityStats.Implementations;

public class RecipeIconStat : ComponentUpdatableEntityStatBase<Sprite?, Manufactory>, IImageStat
{
    public override string Id => "RecipeIcon";

    protected override IEntityStatTracker<Sprite?>? GetComponentTracker(UpdatableEntityStatComponent statComp, Manufactory comp)
        => new RecipeIconStatTracker(comp, statComp);
}

public class RecipeIconStatTracker(Manufactory manufactory, UpdatableEntityStatComponent comp) : StatTrackerBase<Sprite?>(comp), IImageStatTracker
{
    protected override Sprite? CalculateValue() => manufactory.HasCurrentRecipe
        ? manufactory.CurrentRecipe.Icon.Asset
        : null;

    protected override void OnPause() => manufactory.RecipeChanged -= OnRecipeChanged;
    protected override void OnStart() => manufactory.RecipeChanged += OnRecipeChanged;
    void OnRecipeChanged(object sender, EventArgs e) => UpdateValue();
}
