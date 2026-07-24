namespace MoreBuildingRenovations.Renovations;

[BindRenovation]
public class PlantsSpeed(ILoc t, TemplateService templateService) : RenovationBase, ICustomCostRenovation, ILoadableSingleton
{
    static readonly string BonusId = "Renovation_" + nameof(PlantsSpeed);

    public override string Id => nameof(PlantsSpeed);

    ImmutableArray<GoodAmount> cost = [];

    public float SpeedBonus => Spec.Parameters[0];

    public void Load()
    {
        HashSet<string> plantProducts = [];

        var plantables = templateService.GetAll<PlantableSpec>();
        foreach (var p in plantables)
        {
            if (p.GetSpec<CuttableSpec>() is { } cuttable)
            {
                plantProducts.Add(cuttable.Yielder.Yield.Id);
            }

            if (p.GetSpec<GatherableSpec>() is { } gatherable)
            {
                plantProducts.Add(gatherable.Yielder.Yield.Id);
            }
        }

        plantProducts.Remove("Log");

        cost = [.. plantProducts.Select(p => new GoodAmount(p, 1))];
    }

    public IEnumerable<GoodAmount> GetCost() => cost;

    public override string? GetExtraDescription(BuildingRenovationComponent building)
        => t.T("LV.MBR.PlantsSpeedDescExtra." + building.GetComponent<PlantsSpeedUpProvider>().AffectingType);

    public override bool CanRenovate(BuildingRenovationComponent building)
        => building.HasComponent<PlantsSpeedUpSpec>()
        && building.HasComponent<BuildingTerrainRange>();

    public override void OnCompleted(BuildingRenovationComponent building, bool isLoad) 
        => building.GetComponent<PlantsSpeedUpProvider>().Activate(SpeedBonus);
}
