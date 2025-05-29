namespace TailsManager.Models;

public record TailInfo(int Id, string Name, string Author, string FileName)
{

    public string? Description { get; set; }
    public string? Url { get; set; }
    public ImmutableArray<string>? Factions { get; set; }

}

public readonly record struct TailCollection(int Id, string Name, string? Url, ImmutableArray<TailInfo> Tails);

public class TailsMetadata
{
    public ImmutableArray<TailCollection> Collections { get; init; } = [];
}

public class TailsSubscription
{
    public HashSet<int> CollectionIds { get; set; } = [];
    public HashSet<int> TailIds { get; set; } = [];
    public bool IgnoreFactions { get; set; }
}