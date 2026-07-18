namespace HousePainter.Services;

/// <summary>
/// Atlas tile layout from AutoAtlaserSpec (same grid math as the game's AutoAtlaser).
/// </summary>
[BindSingleton]
public class AtlasFragmentCatalog(ISpecService specs) : ILoadableSingleton
{

    ImmutableArray<AtlasFragment> fragments = [];
    Dictionary<string, AtlasFragment> byMaterialName = new(StringComparer.OrdinalIgnoreCase);

    public ImmutableArray<AtlasFragment> Fragments => fragments;

    public void Load()
    {
        List<AtlasFragment> list = [];

        try
        {
            var spec = specs.GetSingleSpec<AutoAtlaserSpec>();
            foreach (var atlas in spec.AutoAtlases)
            {
                var atlasFragments = atlas.Fragments;
                if (atlasFragments.IsEmpty)
                {
                    continue;
                }

                var grid = CalculateGrid(atlasFragments.Length);
                var uvScale = Vector2.one / grid;

                for (var i = 0; i < atlasFragments.Length; i++)
                {
                    var path = atlasFragments[i].Path;
                    var materialName = MaterialNames.Clean(path);
                    var cellX = i % grid;
                    var cellY = i / grid;
                    var fragment = new AtlasFragment(
                        AtlasName: atlas.Name,
                        MaterialName: materialName,
                        MaterialPath: path,
                        UVOffset: new Vector2(cellX / (float)grid, cellY / (float)grid),
                        UVScale: uvScale
                    );
                    list.Add(fragment);
                    byMaterialName[materialName] = fragment;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[HousePainter] Failed to load AutoAtlaserSpec: {ex}");
        }

        fragments = [.. list];
    }

    public bool TryGet(string materialName, out AtlasFragment fragment)
    {
        var cleaned = MaterialNames.Clean(materialName);
        if (byMaterialName.TryGetValue(cleaned, out fragment))
        {
            return true;
        }

        foreach (var (key, value) in byMaterialName)
        {
            if (key.Contains(cleaned, StringComparison.OrdinalIgnoreCase)
                || cleaned.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                fragment = value;
                return true;
            }
        }

        fragment = default;
        return false;
    }

    public ImmutableArray<AtlasFragment> MatchAll(IEnumerable<string> materialNames)
    {
        List<AtlasFragment> matched = [];
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);

        foreach (var name in materialNames)
        {
            if (!TryGet(name, out var fragment))
            {
                continue;
            }

            if (seen.Add(fragment.MaterialName))
            {
                matched.Add(fragment);
            }
        }

        return [.. matched];
    }

    public ImmutableArray<AtlasFragment> ForFactionHint(string templateName)
    {
        var faction = templateName.Contains("IronTeeth", StringComparison.OrdinalIgnoreCase)
            ? "IronTeeth"
            : "Folktails";

        return [.. fragments.Where(f => f.AtlasName.Contains(faction, StringComparison.OrdinalIgnoreCase))];
    }

    static int CalculateGrid(int fragmentCount)
    {
        int i;
        for (i = 1; i * i < fragmentCount; i++)
        {
        }

        return i;
    }

}
