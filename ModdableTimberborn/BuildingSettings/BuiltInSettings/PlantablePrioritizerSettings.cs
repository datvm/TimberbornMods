using SModel = ModdableTimberborn.BuildingSettings.BuiltInSettings.CachableStringSettingModel<Timberborn.Planting.PlantableSpec>;

namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class PlantablePrioritizerSettings(
    TemplateNameMapper templateNameMapper,
    ILoc t
) : BuildingSettingsBase<PlantablePrioritizer, SModel>(t)
{

    public override string DescribeModel(SModel model)
    {
        EnsureModelCached(model);
        return model.CachedDisplay!;
    }

    protected override bool ApplyModel(SModel model, PlantablePrioritizer target)
    {
        var p = EnsureModelCached(model);

        if (p is not null && !target._planterBuilding.AllowedPlantables.Contains(p))
        {
            return false;
        }

        target.PrioritizePlantable(p);
        return true;
    }

    PlantableSpec? EnsureModelCached(SModel model) => model.EnsureCached(t,
        s => templateNameMapper.TryGetTemplate(model, out var t)
            ? t.GetSpec<PlantableSpec>()
            : null,
        GetName);

    string GetName(PlantableSpec p) => p.GetName(t);

    protected override SModel GetModel(PlantablePrioritizer duplicable)
    {
        var v = duplicable.PrioritizedPlantableSpec;
        return new(v?.TemplateName, v, t, GetName);
    }

}