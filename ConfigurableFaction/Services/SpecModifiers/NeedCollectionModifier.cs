namespace ConfigurableFaction.Services.SpecAppenders;

[MultiBind(typeof(ISpecModifier))]
public class NeedCollectionModifier(CurrentFactionSettingsProvider factionProvider) : BaseCollectionModifier<NeedCollectionSpec>(factionProvider)
{
    protected override string GetId(NeedCollectionSpec spec) => spec.CollectionId;

    protected override NeedCollectionSpec ClearCollection(NeedCollectionSpec spec)
        => spec with { Needs = [] };

    protected override NeedCollectionSpec ModifyModCollection(NeedCollectionSpec spec)
        => spec with { Needs = [.. current.Needs] };
}
