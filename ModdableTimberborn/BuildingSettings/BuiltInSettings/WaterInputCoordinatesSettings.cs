namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

#if TIMBERV11
// 1.1 moved the depth-limit fields onto WaterInputPipeCoordinates (water rework). Alias keeps the body version-agnostic.
using WaterInputCoordinates = Timberborn.WaterBuildings.WaterInputPipeCoordinates;
#endif

public record WaterInputCoordinatesSettingsModel(
    bool UseDepthLimit,
    int DepthLimit
);

public class WaterInputCoordinatesSettings(ILoc t) : BuildingSettingsBase<WaterInputCoordinates, WaterInputCoordinatesSettingsModel>(t)
{
    public override string DescribeModel(WaterInputCoordinatesSettingsModel model)
        => model.UseDepthLimit ? (model.DepthLimit + "m") : t.TNone();

    protected override bool ApplyModel(WaterInputCoordinatesSettingsModel model, WaterInputCoordinates target)
    {
        target.UseDepthLimit = model.UseDepthLimit;
#if TIMBERV11
        target.DepthLimit = Math.Min(model.DepthLimit, target._waterInputPipeSpec.MaxDepth);
#else
        target.DepthLimit = Math.Min(model.DepthLimit, target._waterInputSpec.MaxDepth);
#endif
        target.UpdateCoordinatesAndDepth();

        return true;
    }

    protected override WaterInputCoordinatesSettingsModel GetModel(WaterInputCoordinates duplicable) 
        => new(duplicable.UseDepthLimit, duplicable.DepthLimit);
}