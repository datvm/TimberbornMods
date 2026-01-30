namespace ConfigurableFaction.Services.SpecAppenders;

[MultiBind(typeof(ISpecModifier))]
public class GoodCollectionModifier(CurrentFactionSettingsProvider factionProvider) : BaseCollectionModifier<GoodCollectionSpec>(factionProvider)
{
    protected override string GetId(GoodCollectionSpec spec) => spec.CollectionId;

    protected override GoodCollectionSpec ClearCollection(GoodCollectionSpec spec)
        => spec with { Goods = [], };

    protected override GoodCollectionSpec ModifyModCollection(GoodCollectionSpec spec)
        => spec with { Goods = [.. current.Goods] };
}
