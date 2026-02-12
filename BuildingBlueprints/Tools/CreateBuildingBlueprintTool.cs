namespace BuildingBlueprints.Tools;

[BindSingleton]
public class CreateBuildingBlueprintTool(
    ILoc t,
    AreaBlockObjectPickerFactory areaBlockObjectPickerFactory,
    BlockObjectSelectionDrawerFactory blockObjectSelectionDrawerFactory,
    CursorService cursorService,
    InputService inputService,
    ISpecService specService,
    IContainer container,
    BuildingBlueprintsService buildingBlueprintsService
) : ITool, IToolDescriptor, ILoadableSingleton, IInputProcessor
{
#nullable disable
    ToolDescription toolDescription;
    AreaBlockObjectPicker picker;
    BlockObjectSelectionDrawer highlighter;
#nullable enable

    public void Load()
    {
        toolDescription = new ToolDescription.Builder(t.T("LV.BB.BlueprintCreate"))
            .AddSection(t.T("LV.BB.BlueprintCreateDesc"))
            .Build();

        picker = areaBlockObjectPickerFactory.CreatePickingUpwards();

        var colorSpec = specService.GetSingleSpec<BuilderPriorityToolSpec>();
        highlighter = blockObjectSelectionDrawerFactory.Create(colorSpec.PriorityActionColor, colorSpec.PriorityTileColor, colorSpec.PrioritySideColor);
    }

    public ToolDescription DescribeTool() => toolDescription;

    public void Enter()
    {
        inputService.AddInputProcessor(this);
        cursorService.SetCursor("BuildingBlueprintCursor");
    }

    public void Exit()
    {
        cursorService.ResetCursor();
        picker.Reset();
        ClearHighlights();
        inputService.RemoveInputProcessor(this);
    }

    public bool ProcessInput()
        => picker.PickBlockObjects<PlaceableBlockObjectSpec>(PreviewCallback, ActionCallback, ShowNoneCallback);

    void PreviewCallback(IEnumerable<BlockObject> blockObjects, Vector3Int start, Vector3Int end, bool selectionStarted, bool selectingArea)
    {
        if (!selectingArea)
        {
            ClearHighlights();
            return;
        }

        var area = BuildingBlueprintsService.FromAreaSelection(start, end);
        var filtered = blockObjects.Where(b => buildingBlueprintsService.FilterSelection(b, area));
        highlighter.Draw(filtered, start, end, true);
    }

    void ActionCallback(IEnumerable<BlockObject> blockObjects, Vector3Int start, Vector3Int end, bool selectionStarted, bool selectingArea)
    {
        ClearHighlights();

        if (!selectingArea) { return; }

        var area = BuildingBlueprintsService.FromAreaSelection(start, end);
        var baseZ = Math.Min(start.z, end.z);

        var bos = blockObjects.Where(b => buildingBlueprintsService.FilterSelection(b, area));
        var info = BlueprintSelectionInfo.CreateFromSelection(t.T("LV.BB.DefaultName"), [..bos], area, baseZ);
        if (!info.HasAnyBuilding) { return; }

        var diag = container.GetInstance<BlueprintCreationDialog>();
        diag.Show(info);
    }

    void ShowNoneCallback() => ClearHighlights();
    void ClearHighlights() => highlighter.StopDrawing();
}
