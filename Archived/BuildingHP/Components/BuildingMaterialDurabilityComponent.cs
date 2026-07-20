namespace BuildingHP.Components;

public class BuildingMaterialDurabilityComponent : BaseComponent, IBuildingDeltaDurabilityModifier, IBuildingInvulnerabilityModifier
{
    public string DescriptionKey { get; } = "LV.BHP.MaterialDurability";
    public bool Invulnerable { get; private set; }
    public float? ModifierEndTime { get; }
    public int? Delta { get; private set; }

    public event Action<IBuildingDurabilityModifier>? OnChanged;

    BuildingMaterialDurabilityService service = null!;

    [Inject]
    public void Inject(BuildingMaterialDurabilityService service)
    {
        this.service = service;
    }

    public void Initialize()
    {
        var buildingHPSpec = GetComponentFast<BuildingHPComponentSpec>();
        if (buildingHPSpec.NoMaterialDurability) { return; }

        var building = GetComponentFast<BuildingSpec>();
        if (building.BuildingCost.Count == 0)
        {
            Invulnerable = true;
            return;
        }

        Delta = service.GetDurability(building.BuildingCost);
    }
}
