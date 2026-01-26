namespace ConfigurableFaction.Definitions;

public record FactionDef(
    FactionSpec Spec,
    ImmutableArray<BuildingDef> Buildings,
    ImmutableArray<PlantDef> Plants,
    ImmutableArray<NeedSpec> Needs
)
{
    public string Id => Spec.Id;

    public ImmutableArray<BuildingDef> Buildings { get; set; } = [];
    public FrozenDictionary<string, ImmutableArray<BuildingDef>> BuildingsByGroups { get; set; } = FrozenDictionary<string, ImmutableArray<BuildingDef>>.Empty;

    public ImmutableArray<PlantDef> Plants { get; set; } = [];
}