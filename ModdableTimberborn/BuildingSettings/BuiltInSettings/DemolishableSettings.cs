namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class DemolishableSettings(ILoc t) : BuildingSettingsBase<Demolishable, BoolSettingModel>(t)
{
    public override string DescribeModel(BoolSettingModel model) => model.T(t);

    protected override bool ApplyModel(BoolSettingModel model, Demolishable target)
    {
        bool v = model;
        if (target.IsMarked != v)
        {
            if (v)
            {
                target.Mark();
            }
            else
            {
                target.Unmark();
            }
        }

        return true;
    }

    protected override BoolSettingModel GetModel(Demolishable duplicable) => duplicable.IsMarked;
}