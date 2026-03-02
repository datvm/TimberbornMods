namespace BuildingBlueprints.Services;

[BindSingleton]
public class BuildingBlueprintTagService(
    ILoc t
) : ILoadableSingleton
{

    public FrozenDictionary<string, ParsedBlueprintTag> TagsByNames { get; private set; } = FrozenDictionary<string, ParsedBlueprintTag>.Empty;
    public ImmutableArray<ParsedBlueprintTag> Tags { get; private set; } = [];

    public ParsedBlueprintTag Untagged { get; private set; }
    public string UntaggedName { get; private set; } = "";

    public IEnumerable<string> TagNames => Tags.Select(t => t.Name);

    public IEnumerable<string> TagNamesWithoutUntagged => TagNames.Skip(1);

    public void Load()
    {
        UntaggedName = t.T("LV.BB.Untagged");
    }

    public void OnBlueprintCacheUpdated(List<ParsedBlueprintInfo> parsedCache)
    {
        Dictionary<string, List<ParsedBlueprintInfo>> blueprintsByTag = [];
        List<ParsedBlueprintInfo> untaggedList = [];
        var untaggedName = UntaggedName;
        blueprintsByTag.Add(untaggedName, untaggedList);

        foreach (var bp in parsedCache)
        {
            var tags = bp.RawInfo.Tags;

            if (tags.Count == 0)
            {
                untaggedList.Add(bp);
                continue;
            }

            foreach (var tag in tags)
            {
                var list = blueprintsByTag.GetOrAdd(tag, () => []);
                list.Add(bp);
            }
        }

        Tags = [.. blueprintsByTag
            .Select(kv => new ParsedBlueprintTag(
                kv.Key,
                [.. kv.Value.OrderBy(v => v.Name)])
            ).OrderBy(t => t.Name != untaggedName)
            .ThenBy(t => t.Name)
        ];

        TagsByNames = Tags.ToFrozenDictionary(t => t.Name);

        Untagged = TagsByNames[untaggedName];
    }

}
