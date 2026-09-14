namespace TimberPipes.Services;

public readonly record struct ValvePipeBuildingSettingModel(bool InletEnabled, bool OutletEnabled, string? OutletGoodId);

[BindBuildingSettings]
public class ValvePipeBuildingSettings(ILoc t, IGoodService goods)
    : BuildingSettingsBase<ValvePipe, ValvePipeBuildingSettingModel>(t)
{
    public override string DescribeModel(ValvePipeBuildingSettingModel model)
    {
        var extract = model.OutletEnabled && model.OutletGoodId is { Length: > 0 } id && goods.HasGood(id)
            ? goods.GetGood(id).DisplayName.Value
            : t.TYesNo(model.OutletEnabled);
        return t.T("LV.TPi.BldSet.Desc", t.TYesNo(model.InletEnabled), extract);
    }

    protected override bool ApplyModel(ValvePipeBuildingSettingModel model, ValvePipe target)
    {
        target.CopySettings(model.InletEnabled, model.OutletEnabled, model.OutletGoodId);
        return true;
    }

    protected override ValvePipeBuildingSettingModel GetModel(ValvePipe duplicable)
        => new(duplicable.InletEnabled, duplicable.OutletEnabled, duplicable.OutletGoodId);
}
