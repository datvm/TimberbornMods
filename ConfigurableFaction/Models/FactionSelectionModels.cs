namespace ConfigurableFaction.Models;

public class FactionOptions(string id)
{
    public string Id { get; } = id;

    public HashSet<string> Buildings { get; init; } = [];
    public HashSet<string> Plantables { get; init; } = [];
    public HashSet<string> Needs { get; init; } = [];
    public HashSet<string> Goods { get; init; } = [];

    public HashSet<string> SpecialBuildings { get; init; } = [];

    [JsonIgnore]
    public HashSet<string> LockedInNeeds { get; } = [];
    [JsonIgnore]
    public HashSet<string> LockedInGoods { get; } = [];
    [JsonIgnore]
    public HashSet<string> LockedOutPlantGroup { get; } = [];

}