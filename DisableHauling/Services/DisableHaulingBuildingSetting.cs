using ModdableTimberborn.BuildingSettings;
using ModdableTimberborn.BuildingSettings.BuiltInSettings;

namespace DisableHauling.Services;

[MultiBind(typeof(IBuildingSettings))]
public class DisableHaulingBuildingSetting(ILoc t) : BuildingSettingsBase<DisableHaulingComponent, BoolSettingModel>(t)
{
    public override string DescribeModel(BoolSettingModel model) => t.TYesNo(model);

    protected override bool ApplyModel(BoolSettingModel model, DisableHaulingComponent target)
    {
        target.DisableHauling = model;
        return true;
    }

    protected override BoolSettingModel GetModel(DisableHaulingComponent duplicable)
        => duplicable.DisableHauling;
}
