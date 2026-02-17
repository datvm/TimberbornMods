using SModel = ModdableTimberborn.BuildingSettings.BuiltInSettings.CachableStringSettingModel<Timberborn.Goods.GoodSpec>;

namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class SingleGoodAllowerSettings(
    IGoodService goods,
    ILoc t
) : BuildingSettingsBase<SingleGoodAllower, SModel>(t)
{
    public override string DescribeModel(SModel model)
    {
        EnsureModelCached(model);
        return model.CachedDisplay!;
    }

    protected override bool ApplyModel(SModel model, SingleGoodAllower target)
    {
        var id = model.Value;
        if (id is not null && !target._inventory.Takes(id))
        {
            return false;
        }

        target.Allow(id);
        return true;
    }

    void EnsureModelCached(SModel model)
    {
        model.EnsureCached(t,
            id => goods.GetGoodOrNull(id),
            s => s.DisplayName.Value);
    }

    protected override SModel GetModel(SingleGoodAllower duplicable) => new(duplicable.AllowedGood);


}