namespace BuildingHP.Services;

public class BuildingMaterialDurabilityService(
    ISpecService specs,
    ILoc t,
    MSettings s
) : ILoadableSingleton
{
    public FrozenDictionary<string, int> Materials { get; private set; } = FrozenDictionary<string, int>.Empty;

    public void Load()
    {
        var ss = specs.GetSpecs<BuildingMaterialDurabilitySpec>();
        Materials = ss.ToFrozenDictionary(s => s.GoodId, s => s.Durability);
    }

    public int GetDurability(string goodId) => Materials.TryGetValue(goodId, out var durability) ? durability : 0;

    public int GetDurability(IEnumerable<GoodAmountSpec> buildingCosts)
    {
        var totalAmount = 0;
        var totalWeight = 0;
        foreach (var c in buildingCosts)
        {
            var d = GetDurability(c.GoodId);
            if (d == 0) { continue; }

            totalAmount += c.Amount;
            totalWeight += d * c.Amount;
        }

        return Math.Max(0, Mathf.CeilToInt((float)totalWeight / totalAmount));
    }

    public string GetDisplayedBaseDurability(BuildingHPComponent comp)
    {
        var spec = comp.GetComponentFast<BuildingHPComponentSpec>();
        var d = spec.BaseDurability;

        var invul = false;
        if (s.EnableMaterialDurability.Value && !spec.NoMaterialDurability)
        {
            var building = comp.GetComponentFast<BuildingSpec>();
            if (building)
            {
                var cost = building.BuildingCost;
                if (cost.Count == 0)
                {
                    invul = true;
                }
                else
                {
                    d += GetDurability(cost);
                }
            }
        }

        return t.T(invul ? "LV.BHP.BaseDurabilityWithInvul" : "LV.BHP.BaseDurability", d);
    }

}
