namespace BuildingBlueprints.Tools;

[BindSingleton]
public class DemolishBlueprintTool(
    ILoc t,
    SelectableObjectRaycaster selectableObjectRaycaster,
    BlueprintGroupService groupService,
    CursorService cursorService,
    InputService inputService,
    DialogService diag,
    DestructionService destructionService
) : ITool, IToolDescriptor, ILoadableSingleton, IInputProcessor, IConstructionModeEnabler
{
#nullable disable
    ToolDescription toolDescription;
#nullable enable

    public void Load()
    {
        toolDescription = new ToolDescription.Builder(t.T("LV.BB.DemolishBlueprintTool"))
            .AddSection(t.T("LV.BB.DemolishBlueprintToolDesc"))
            .Build();
    }

    public void Enter()
    {
        cursorService.SetCursor("DeleteBuildingCursor");
        inputService.AddInputProcessor(this);
    }

    public void Exit()
    {
        destructionService.UnhighlightDestructionEntities();
        cursorService.ResetCursor();
        inputService.RemoveInputProcessor(this);
    }

    public ToolDescription DescribeTool() => toolDescription;

    public bool ProcessInput()
    {
        if (!selectableObjectRaycaster.TryHitSelectableObject(out var obj)
            || !obj) { goto NONE; }

        var comp = obj.GetComponent<BuildingBlueprintComponent>();
        if (!comp || !comp.HasGroup) { goto NONE; }

        var buildings = groupService.GetGroup(comp);
        if (buildings.Count == 0) { goto NONE; }

        var destroying = destructionService.QueryDestructingEntities(buildings.Select(b => b.GetComponent<BlockObject>()));
        destructionService.HighlightDestructionEntities(destroying);
        
        if (inputService.MainMouseButtonDown)
        {
            AttemptDelete(destroying);
            return true;
        }

        return false;

    NONE:
        destructionService.UnhighlightDestructionEntities();
        return false;
    }

    async void AttemptDelete(DestroyingEntities destroying)
    {
        if (!await diag.ConfirmAsync(t.T("LV.BB.DemolishConfirm", destroying.BlockObjects.Length))) { return; }
        destructionService.DestroyEntities(destroying);
    }

}
