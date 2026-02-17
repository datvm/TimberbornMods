namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class UnstableCoreSettings(ILoc t) : BuildingSettingsBase<UnstableCore, IntSettingModel>(t)
{
    public override string DescribeModel(IntSettingModel model) => t.T("LV.MT.BldSet.UnstableCore.Desc", model.Value);

    protected override bool ApplyModel(IntSettingModel model, UnstableCore target)
    {
        target.SetRadius(model);
        return true;
    }

    protected override IntSettingModel GetModel(UnstableCore duplicable) => duplicable.ExplosionRadius;
}