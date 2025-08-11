
namespace BuildingHP.Services;

public class BuildingMaterialDurabilityService(ISpecService specs) : ILoadableSingleton
{
    public FrozenDictionary<string, int> Materials { get; private set; } = FrozenDictionary<string, int>.Empty;

    public void Load()
    {
        var ss = specs.GetSpecs<BuildingMaterialDurabilitySpec>();
        Materials = ss.ToFrozenDictionary(s => s.GoodId, s => s.Durability);
    }

    public int GetDurability(string goodId) => Materials.TryGetValue(goodId, out var durability) ? durability : 0;

}
