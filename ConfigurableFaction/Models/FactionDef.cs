namespace ConfigurableFaction.Models;

public record FactionDef(
    FactionSpec Spec,
    ImmutableArray<BuildingDef> Buildings,
    ImmutableArray<PlantDef> Plants,
    ImmutableArray<GoodDef> Goods,
    ImmutableArray<NeedDef> Needs
)
{
    public string Id => Spec.Id;

    public FrozenDictionary<string, ImmutableArray<BuildingDef>> BuildingsByGroups { get; }
        = Buildings.GroupBy(b => b.GroupId).ToFrozenDictionary(
            g => g.Key,
            g => g.ToImmutableArray());

}