namespace BuildingBlueprints.Tools;

[BindSingleton]
public class BuildBuildingBlueprintTool(
    IContainer container,
    BlueprintPlacementService blueprintPlacementService,
    ToolService toolService,
    ILoc t
) : ITool, IToolDescriptor, ILoadableSingleton
{
#nullable disable
    ToolDescription toolDescription;
#nullable enable

    public void Load()
    {
        toolDescription = new ToolDescription.Builder(t.T("LV.BB.BlueprintBuildTool"))
            .AddSection(t.T("LV.BB.BlueprintBuildToolDesc"))
            .AddPrioritizedSection(t.T("LV.BB.BlueprintBuildToolTip"))
            .Build();
    }

    public ToolDescription DescribeTool() => toolDescription;

    public async void Enter()
    {
        var diag = container.GetInstance<BlueprintSelectionDialog>();
        var blueprint = await diag.PickAsync();
        if (blueprint is null)
        {
            container.GetInstance<ToolService>().SwitchToDefaultTool();
            toolService.SwitchToDefaultTool();
            return;
        }

        while (await blueprintPlacementService.PlaceAsync(blueprint) is not null) { }
    }

    public void Exit()
    {
        blueprintPlacementService.Stop();
    }

}
