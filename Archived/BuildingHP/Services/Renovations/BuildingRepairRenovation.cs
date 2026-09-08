namespace BuildingHP.Services.Renovations;

public class BuildingRepairRenovation : BuildingRenovation
{
    private readonly BuildingRepairInfo repairInfo;

    public BuildingRepairRenovation(
        BuildingRenovationComponent building,
        BuildingRepairInfo repairInfo,
        int amount,
        RenovationSpec spec,
        BuildingRenovationDependencies di
    ) : base(building, spec, di)
    {
        this.repairInfo = repairInfo;
        Amount = amount;

        Cost = GetRepairGoods(repairInfo, amount);
        AddToActiveListOnComplete = false;
    }

    public int Amount { get; }

    public static IReadOnlyList<GoodAmountSpecNew> GetRepairGoods(BuildingRepairInfo repairInfo, int amount) 
        => [new() {
            Id = repairInfo.Good,
            Amount = repairInfo.Amount * amount,            
        }];

    protected override void ProcessOnCompleted()
    {
        base.ProcessOnCompleted();

        var buildingHp = Building.BuildingHPComponent;
        buildingHp.Heal(Mathf.CeilToInt(buildingHp.Durability * Amount * repairInfo.HPPercent));
    }

}
