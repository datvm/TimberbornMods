namespace BuildingBlueprints.Tools;

[BindSingleton]
public class BuildBuildingBlueprintTool(
    IContainer container,
    InputService inputService
) : ITool, ILoadableSingleton, IInputProcessor
{

    ParsedBlueprintInfo? placingBlueprint;

    public void Load()
    {
        throw new NotImplementedException();
    }

    public async void Enter()
    {
        var diag = container.GetInstance<BlueprintSelectionDialog>();
        var blueprint = await diag.PickAsync();
        if (blueprint is null)
        {
            container.GetInstance<ToolService>().SwitchToDefaultTool();
        }

        placingBlueprint = blueprint;
        inputService.AddInputProcessor(this);
    }

    public void Exit()
    {
        placingBlueprint = null;
        inputService.RemoveInputProcessor(this);
    }

    public bool ProcessInput()
    {
        if (placingBlueprint is null) // Should not happen
        {
            Exit();
            return false; 
        }

        throw new NotImplementedException();
    }
}
