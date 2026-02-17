using SModel = ModdableTimberborn.BuildingSettings.BuiltInSettings.CachableStringSettingModel<Timberborn.Gathering.GatherableSpec>;

namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class GatherablePrioritizerSettings(
    ILoc t,
    TemplateNameMapper templateNameMapper,
    IGoodService goods
) : BuildingSettingsBase<GatherablePrioritizer, SModel>(t)
{
    public override string DescribeModel(SModel model)
    {
        EnsureModelCache(model);
        return model.CachedDisplay!;
    }

    protected override bool ApplyModel(SModel model, GatherablePrioritizer target)
    {
        var gatherable = EnsureModelCache(model);

        if (gatherable is null || target.SupportsGatherable(gatherable))
        {
            target.PrioritizeGatherable(gatherable);
            return true;
        }

        return false;
    }

    GatherableSpec? EnsureModelCache(SModel model) => model.EnsureCached(t,
        templateName => templateNameMapper.TryGetTemplate(templateName, out var spec)
            ? spec.GetSpec<GatherableSpec>()
            : null,
        GetYieldName
    );

    string GetYieldName(GatherableSpec spec) => goods.GetGood(spec.Yielder.Yield.Id).DisplayName.Value;

    protected override SModel GetModel(GatherablePrioritizer duplicable)
    {
        var g = duplicable.PrioritizedGatherable;
        return new(g?.GetTemplateName(), g, t,GetYieldName);
    }
}