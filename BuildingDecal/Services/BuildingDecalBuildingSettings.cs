namespace BuildingDecal.Services;

public class BuildingDecalBuildingSettings(ILoc t) : BuildingSettingsBase<BuildingDecalComponent, BuildingDecalBuildingSettingsModel>(t)
{
    public override string DescribeModel(BuildingDecalBuildingSettingsModel model)
        => t.T("LV.BDl.BannersDesc", model.Decals.Length);

    protected override bool ApplyModel(BuildingDecalBuildingSettingsModel model, BuildingDecalComponent target) 
        => target.SetTo(model.Decals);

    protected override BuildingDecalBuildingSettingsModel GetModel(BuildingDecalComponent duplicable) 
        => BuildingDecalBuildingSettingsModel.From(duplicable);
}
