namespace ModdableDecalGroups.Specs;

public record DecalGroupSpec : ComponentSpec
{

#nullable disable
    [Serialize]
    public string Id { get; init; }

    [Serialize]
    public string TitleLoc { get; init; }

    [Serialize(nameof(TitleLoc))]
    public LocalizedText Title { get; init; }

    [Serialize]
    public string Category { get; init; }
#nullable enable

    [Serialize]
    public int Order { get; init; }

    [Serialize]
    public ImmutableArray<string> DecalIdPatterns { get; init; } = [];

    [Serialize]
    public ImmutableArray<string> DecalIdExacts { get; init; } = [];

    public bool IsDefault => Id == "Default" + Category;

    ImmutableArray<Regex>? patterns;
    public ImmutableArray<Regex> DecalIdsRegExs 
        => patterns ??= [.. DecalIdPatterns
            .Select(p => new Regex(p, RegexOptions.Compiled | RegexOptions.IgnoreCase))];
}
