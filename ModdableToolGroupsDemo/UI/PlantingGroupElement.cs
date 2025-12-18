
namespace ModdableToolGroupsDemo.UI;

public class PlantingGroupElement(
    PlantingToolButtonFactory plantingToolButtonFactory,
    TemplateService templateService,

    ToolGroupService toolGroupService,
    ModdableToolGroupButtonFactory buttonFac
) : CustomRootToolGroupElement(toolGroupService, buttonFac), ILoadableSingleton, IHotkeySupportedTool
{
    public override string Id { get; } = "LV.PlantingGroup";
    const string GroupId = "Plantings";

    public ImmutableArray<PlantableSpec> Fields { get; private set; } = [];
    public ImmutableArray<PlantableSpec> Forestry { get; private set; } = [];
    readonly List<IToolHotkeyDefinition> hotkeys = [];

    public IEnumerable<IToolHotkeyDefinition> GetHotkeys() => hotkeys;

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

    int order = 10;
    protected override void AddChildren(ModdableToolGroupButton btn)
    {
        // Add hotkey for the root button
        hotkeys.Add(new ButtonToolHotkeyDefinition($"Planting.Tool.Root", "LV.MTGD.PlantingGroup", btn.ToolGroupButton)
        {
            Order = order += 10,
            GroupId = GroupId,
        });

        CreateButtonFor(btn, toolGroupService.GetGroup(FieldsButton.ToolGroupId), Fields);
        CreateButtonFor(btn, toolGroupService.GetGroup(ForestryButton.ToolGroupId), Forestry);

        // Cancel planting button
        var cancelBtn = plantingToolButtonFactory.CreateCancelTool(btn.ToolButtonsElement);
        btn.AddChildTool(cancelBtn);
        hotkeys.Add(new ButtonToolHotkeyDefinition($"Planting.Tool.Cancel", CancelPlantingTool.TitleLocKey, cancelBtn)
        {
            Order = order += 10,
            GroupId = GroupId,
        });
    }

    void CreateButtonFor(ModdableToolGroupButton parent, ToolGroupSpec spec, ImmutableArray<PlantableSpec> plantables)
    {
        // The group button
        var btn = parent.AddChildGroup(spec);
        hotkeys.Add(new ButtonToolHotkeyDefinition($"Planting.Tool.{spec.Id}", spec.DisplayNameLocKey, btn.ToolGroupButton)
        {
            Order = order += 10,
            GroupId = GroupId,
        });
        var btns = btn.ToolButtonsElement;

        // The plant buttons
        foreach (var plantable in plantables)
        {
            var toolBtn = plantingToolButtonFactory.CreatePlantingTool(plantable, btns);
            btn.AddChildTool(toolBtn);

            hotkeys.Add(new ButtonToolHotkeyDefinition(
                $"Planting.Plant.{plantable.TemplateName}",
                plantable.GetSpec<LabeledEntitySpec>().DisplayNameLocKey,
                toolBtn
            )
            {
                Order = order += 10,
                GroupId = GroupId,
            });
        }
    }

}
