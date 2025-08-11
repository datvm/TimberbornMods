namespace BuildingHP.Components;

public class BuildingMaterialDurabilityComponent : BaseComponent, IBuildingDurabilityModifier
{
    public float? Multiplier { get; }
    public int? Delta { get; private set; }
    public bool Invulnerable { get; private set; }

    public event Action<IBuildingDurabilityModifier>? OnChanged;

    BuildingMaterialDurabilityService service = null!;

    [Inject]
    public void Inject(BuildingMaterialDurabilityService service)
    {
        this.service = service;
    }

    public void Awake()
    {
        var buildingHPSpec = GetComponentFast<BuildingHPComponentSpec>();
        if (buildingHPSpec.NoMaterialDurability) { return; }

        var building = GetComponentFast<BuildingSpec>();
        if (building.BuildingCost.Count == 0)
        {
            Invulnerable = true;
            return;
        }

        var totalAmount = 0;
        var totalWeight = 0;
        foreach (var c in building.BuildingCost)
        {
            var d = service.GetDurability(c.GoodId);
            if (d == 0) { continue; }

            totalAmount += c.Amount;
            totalWeight += d * c.Amount;
        }

        var durability = Mathf.CeilToInt((float)totalWeight / totalAmount);

        Delta = Math.Max(
            0, // Cap at 0 in case there are too many Gears as it's negative
            durability);
    }

}
