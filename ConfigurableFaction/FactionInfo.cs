namespace ConfigurableFaction;

public record FactionInfo(FactionSpec Faction, ImmutableArray<SimpleBuildingSpec> Buildings)
{

    [JsonIgnore]
    public bool Enabled { get; set; }

    public string Id => Faction.Id;

}

public record SimpleBuildingSpec(string Id, string NameKey, bool IsCommon)
{

    [JsonIgnore]
    public bool Enabled { get; set; }
}