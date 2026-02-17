namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record FixedStockpileInventorySetterSettingsModel(string Id, int Amount);
public class FixedStockpileInventorySetterSettings(
    ILoc t,
    IGoodService goods
) : BuildingSettingsBase<FixedStockpileInventorySetter, FixedStockpileInventorySetterSettingsModel>(t)
{
    public override string DescribeModel(FixedStockpileInventorySetterSettingsModel model)
        => $"{goods.GetGood(model.Id).DisplayName.Value} ×{model.Amount}";

    protected override bool ApplyModel(FixedStockpileInventorySetterSettingsModel model, FixedStockpileInventorySetter target)
    {
        if (target._stockpile.WhitelistedGoodType != model.Id) { return false; }

        target.SetGoodId(model.Id);
        target.SetAmount(model.Amount);
        return true;
    }
    
    protected override FixedStockpileInventorySetterSettingsModel GetModel(FixedStockpileInventorySetter duplicable)
        => new(GetGoodId(duplicable), GetAmount(duplicable));

    static string GetGoodId(FixedStockpileInventorySetter target) => target._singleGoodAllower.AllowedGood;
    static int GetAmount(FixedStockpileInventorySetter target) => target._stockpile.Inventory.TotalAmountInStock;

}