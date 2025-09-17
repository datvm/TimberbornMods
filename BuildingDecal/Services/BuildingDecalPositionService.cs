namespace BuildingDecal.Services;

public class BuildingDecalPositionService(
    ISpecService specs
) : ILoadableSingleton
{

    public FrozenDictionary<string, DecalPositionSpec> Positions { get; private set; } = FrozenDictionary<string, DecalPositionSpec>.Empty;
    public ImmutableArray<DecalPositionSpec> DefaultPositions { get; private set; } = [];

    FrozenDictionary<string, ImmutableArray<DecalPositionSpec>> buildingSpecifics = FrozenDictionary<string, ImmutableArray<DecalPositionSpec>>.Empty;

    public void Load()
    {
        Positions = specs.GetSpecs<DecalPositionSpec>()
            .ToFrozenDictionary(q => q.Id);

        DefaultPositions = [.. Positions.Values
            .Where(q => q.IsDefault)
            .OrderBy(q => q.Order)];

        BuildBuildingSpecificList();
    }

    void BuildBuildingSpecificList()
    {
        // First, group by prefab name
        var buildingPosSpecs = specs.GetSpecs<BuildingDecalPositionSpec>();
        Dictionary<string, List<BuildingDecalPositionSpec>> dict = [];
        foreach (var s in buildingPosSpecs)
        {
            foreach (var prefab in s.PrefabNames)
            {
                dict.GetOrAdd(prefab).Add(s);
            }
        }

        // Then, build the list for each building
        List<KeyValuePair<string, ImmutableArray<DecalPositionSpec>>> list = [];
        foreach (var (prefab, posSpecs) in dict)
        {
            HashSet<DecalPositionSpec> prefabSpecs = [];
            var clearDefault = false;

            foreach (var posSpec in posSpecs)
            {
                prefabSpecs.AddRange(posSpec.Positions.Select(q => Positions[q]));
                if (posSpec.ClearDefaults) { clearDefault = true; }
            }

            if (!clearDefault)
            {
                prefabSpecs.AddRange(DefaultPositions);
            }

            list.Add(new(prefab, [.. prefabSpecs.OrderBy(q => q.Order)]));
        }

        buildingSpecifics = list.ToFrozenDictionary();
    }

    public ImmutableArray<DecalPositionSpec> GetPositionsForBuilding(string prefabName)
        => buildingSpecifics.TryGetValue(prefabName, out var list) ? list : DefaultPositions;

}
