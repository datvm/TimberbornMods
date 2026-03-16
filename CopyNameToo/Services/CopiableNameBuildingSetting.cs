namespace CopyNameToo.Services;

[MultiBind(typeof(IBuildingSettings))]
public class CopiableNameBuildingSetting(ILoc t) : BuildingSettingsBase<CopiableNameComponent, StringSettingModel>(t), IBuildingSettings
{
    int IBuildingSettings.Order => 0;

    public override string DescribeModel(StringSettingModel model) => model.Value;

    protected override bool ApplyModel(StringSettingModel model, CopiableNameComponent target) 
        => target.SetEntityName(model.Value);

    protected override StringSettingModel GetModel(CopiableNameComponent duplicable) => duplicable.EntityName;
}
