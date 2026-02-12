namespace BuildingBlueprints.UI;

public class BuildingBlueprintsButtons(
    CreateBuildingBlueprintTool createBuildingBlueprintTool,
    BuildBuildingBlueprintTool buildBuildingBlueprintTool,

    ToolGroupService toolGroupService,
    ModdableToolGroupButtonFactory grpButtonFac
) : CustomRootToolGroupElement(toolGroupService, grpButtonFac)
{
    public override string Id { get; } = "BuildingBlueprints";

    protected override void AddChildren(ModdableToolGroupButton btn)
    {
        btn.AddChildTool(createBuildingBlueprintTool, "BuildingBlueprintsCreate");
        btn.AddChildTool(buildBuildingBlueprintTool, "BuildingBlueprintsBuild");
    }
}
