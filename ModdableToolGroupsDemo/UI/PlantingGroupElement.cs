namespace ModdableToolGroupsDemo.UI;

public class PlantingGroupElement(
    PlantingToolButtonFactory plantingToolButtonFactory,
    TemplateService templateService,

    ToolGroupService toolGroupService,
    ModdableToolGroupButtonFactory buttonFac
) : CustomRootToolGroupElement(toolGroupService, buttonFac), ILoadableSingleton
{
    public override string Id { get; } = "LV.PlantingGroup";

    public ImmutableArray<PlantableSpec> Fields { get; private set; } = [];
    public ImmutableArray<PlantableSpec> Forestry { get; private set; } = [];

    public void Load()
    {
        var plantables = templateService.GetAll<PlantableSpec>()
            .Select(q => (Plant: q, NaturalResource: q.GetSpec<NaturalResourceSpec>()))
            .Where(q => q.NaturalResource.UsableWithCurrentFeatureToggles)
            .OrderBy(q => q.NaturalResource.Order)
            .ToArray();

        Fields = [.. plantables
            .Where(q => q.Plant.HasSpec<CropSpec>())
            .Select(q => q.Plant)];
        Forestry = [.. plantables
            .Where(q => q.Plant.HasSpec<BushSpec>())
            .Concat(plantables.Where(q => q.Plant.HasSpec<TreeComponentSpec>()))
            .Select(q => q.Plant)];
    }

    protected override void AddChildren(ModdableToolGroupButton btn)
    {
        CreateButtonFor(btn, toolGroupService.GetGroup(FieldsButton.ToolGroupId), Fields);
        CreateButtonFor(btn, toolGroupService.GetGroup(ForestryButton.ToolGroupId), Forestry);
    }

    void CreateButtonFor(ModdableToolGroupButton parent, ToolGroupSpec spec, ImmutableArray<PlantableSpec> plantables)
    {
        var btn = parent.AddChildGroup(spec);
        var btns = btn.ToolButtonsElement;

        foreach (var plantable in plantables)
        {
            var tool = plantingToolButtonFactory.CreatePlantingTool(plantable, btns);
            btn.AddChildTool(tool);
        }
    }

}
