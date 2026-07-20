namespace BuildingHP.Services;

public class BuildingRepairService(
    BuildingMaterialDurabilityService buildingMaterialDurabilityService,
    RenovationSpecService renovationSpecService,
    BuildingRenovationDependencies di
) : ILoadableSingleton
{
    public const float FullRepairAmountMultiplier = .5f;
    public const float RepairMinPortion = .1f;
    public const float RepairMaxPortion = .5f;

    public RenovationSpec RepairRenovationSpec { get; private set; } = null!;

    public BuildingRepairInfo CalculateRepairCost(IEnumerable<GoodAmountSpec> buildingCost)
    {
        if (!buildingCost.Any()) { return BuildingRepairInfo.OneLog; }

        GoodAmountSpec curr = default;
        foreach (var c in buildingCost)
        {
            if (c.Amount < curr.Amount) { continue; }

            if (c.Amount == curr.Amount
                && buildingMaterialDurabilityService.GetDurability(c.GoodId) <= buildingMaterialDurabilityService.GetDurability(curr.GoodId))
            {
                continue;
            }

            curr = c;
        }

        // Material units to go from 0% -> 100% HP
        var fullRepairAmount = curr.Amount * FullRepairAmountMultiplier;
        if (fullRepairAmount <= 0) { return BuildingRepairInfo.OneLog; }

        // Minimum portion player can spend (rounded up to whole material units)
        var portion = Mathf.CeilToInt(fullRepairAmount * RepairMinPortion);

        // Actual HP% restored by spending "portion" materials; clamp to max portion cap
        var portionHpPerc = Mathf.Min(RepairMaxPortion, portion / fullRepairAmount); // e.g. 1 / 5 = 0.2 (20%)

        return new(curr.GoodId, portion, portionHpPerc);
    }

    public void Load()
    {
        RepairRenovationSpec = renovationSpecService.Renovations[RepairRenovationProvider.RenovationId];
    }

    public void Repair(BuildingHPRepairComponent comp, int amount, Priority priority)
    {
        if (amount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive");
        }

        var reno = comp.GetRenovationComponent();
        reno.Renovate(new BuildingRepairRenovation(reno, comp.RepairInfo, amount, RepairRenovationSpec, di), priority);
    }

}
