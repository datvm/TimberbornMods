namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record ResourceCounterSettingsModel(
    ResourceCounterMode Mode,
    string? GoodId,
    int Threshold,
    float FillRateThreshold,
    NumericComparisonMode ComparisonMode,
    bool IncludeInputs
);

public class ResourceCounterSettings(
    ILoc t,
    IGoodService goods
) : BuildingSettingsBase<ResourceCounter, ResourceCounterSettingsModel>(t)
{
    public override string DescribeModel(ResourceCounterSettingsModel model)
    {
        var good = (GetGood(model.GoodId)?.DisplayName.Value)
            ?? t.TNone();
        
        return $"{good} {model.ComparisonMode.ToChar()} "
            + (model.Mode == ResourceCounterMode.StockLevel ? model.Threshold.ToString() : model.FillRateThreshold.ToString("0%"));
    }

    protected override bool ApplyModel(ResourceCounterSettingsModel model, ResourceCounter target)
    {
        target.Mode = model.Mode;
        var id = model.GoodId;
        if (GetGood(id) is not null)
        {
            target.GoodId = id;
        }
        target.IncludeInputs = model.IncludeInputs;
        target.InvokeGoodChangeEvent(id);

        target.Threshold= model.Threshold;
        target.FillRateThreshold = model.FillRateThreshold;
        target.ComparisonMode = model.ComparisonMode;

        target.Sample();

        return true;
    }

    GoodSpec? GetGood(string? id) => id is null ? null : goods.GetGoodOrNull(id);

    protected override ResourceCounterSettingsModel GetModel(ResourceCounter target)
        => new(target.Mode, target.GoodId, target.Threshold, target.FillRateThreshold, target.ComparisonMode, target.IncludeInputs);
}