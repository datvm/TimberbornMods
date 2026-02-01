
namespace ModdableDecalGroups.Services;

[BindSingleton]
public class DecalGroupService(
    ISpecService specs,
    IDecalService decalService
) : ILoadableSingleton
{
    readonly DecalService decalService = (DecalService)decalService;

#nullable disable
    FrozenDictionary<string, DecalCategoryGroups> groupsInCategories;
    FrozenDictionary<string, DecalGroupSpec> groupsByIds;
    FrozenDictionary<string, DecalGroupSpec> defaultGroups;

    public DecalGroupSpec DefaultBanners { get; private set; }
    public DecalGroupSpec DefaultTails { get; private set; }
#nullable enable

    public DecalCategoryGroups GetGroups(string category) => groupsInCategories[category];
    public DecalGroupSpec GetGroupSpec(string groupId) => groupsByIds[groupId];
    
    public void Load()
    {
        Dictionary<string, List<DecalGroup>> groupsInCategories = [];
        Dictionary<string, DecalGroupSpec> defaultGroups = [];
        foreach (var k in decalService._decalCategories.Keys)
        {
            groupsInCategories[k] = [];
        }

        Dictionary<string, HashSet<string>> decalsInCategories = decalService._decalCategories
            .ToDictionary(kv => kv.Key, kv => kv.Value._categorySpecs.Keys.ToHashSet());
        Dictionary<string, HashSet<string>> unprocessedCategories = decalsInCategories
            .ToDictionary(kv => kv.Key, kv => new HashSet<string>(kv.Value));

        Dictionary<string, DecalGroupSpec> groupsByIds = [];
        foreach (var spec in specs.GetSpecs<DecalGroupSpec>())
        {
            if (groupsByIds.TryGetValue(spec.Id, out var existing))
            {
                throw new ArgumentException($"[{nameof(ModdableDecalGroups)}] Duplicate decal group ID: {spec.Id}, declared by " +
                    $"{spec.Blueprint.Name} and {existing.Blueprint.Name}");
            }
            groupsByIds[spec.Id] = spec;

            var cat = spec.Category;
            if (!groupsInCategories.TryGetValue(cat, out var groups))
            {
                Debug.LogWarning($"[{nameof(ModdableDecalGroups)}] Unknown category: {cat} (from spec {spec.Id})");
                continue;
            }

            if (spec.IsDefault)
            {
                defaultGroups.Add(cat, spec);
                continue;
            }

            var decals = decalsInCategories[cat];
            HashSet<string> matchings = [];

            foreach (var m in spec.DecalIdExacts)
            {
                if (decals.Contains(m))
                {
                    matchings.Add(m);
                }
            }

            foreach (var pattern in spec.DecalIdsRegExs)
            {
                foreach (var decalId in decals)
                {
                    if (pattern.IsMatch(decalId))
                    {
                        matchings.Add(decalId);
                    }
                }
            }

            if (matchings.Count > 0)
            {
                groupsInCategories[cat].Add(new(spec, ToDecalArray(matchings, cat)));
                foreach (var m in matchings)
                {
                    unprocessedCategories[cat].Remove(m);
                }
            }
        }

        foreach (var (cat, spec) in defaultGroups)
        {
            var remaining = unprocessedCategories[cat];
            if (remaining.Count == 0) { continue; }

            groupsInCategories[cat].Add(new(spec, ToDecalArray(remaining, cat)));
        }

        this.groupsInCategories = groupsInCategories
            .ToFrozenDictionary(
                kv => kv.Key,
                kv => new DecalCategoryGroups(kv.Key, [..kv.Value.OrderBy(q => q.Spec.Order)])
            );
        this.groupsByIds = groupsByIds.ToFrozenDictionary();
        this.defaultGroups = defaultGroups.ToFrozenDictionary();

        DefaultBanners = this.defaultGroups["Banners"];
        DefaultTails = this.defaultGroups["Tails"];
    }

    static ImmutableArray<Decal> ToDecalArray(IEnumerable<string> ids, string category) 
        => [.. ids.Select(id => new Decal(id, category))];

}

public record DecalCategoryGroups(string Category, ImmutableArray<DecalGroup> Groups);
public record DecalGroup(DecalGroupSpec Spec, ImmutableArray<Decal> Decals);