namespace BuildingBlueprints.Tools;

[BindSingleton]
public class BuildBuildingBlueprintTool(
    IContainer container,
    BlueprintPlacementService blueprintPlacementService,
    ToolService toolService
) : ITool
{

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

        while (true)
        {
            var placement = await blueprintPlacementService.PickAreaAsync(blueprint);
            if (placement is null) { break; }


        }
    }

    public void Exit()
    {
        blueprintPlacementService.Stop();
    }

}
