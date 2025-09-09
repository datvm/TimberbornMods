namespace MacroManagement.Services;

public class MultiselectTool(
    AreaBlockObjectPickerFactory areaBlockObjectPickerFactory,
    BlockObjectSelectionDrawerFactory blockObjectSelectionDrawerFactory,
    InputService inputService,
    CursorService cursorService,
    MultiSelectService multiSelectService,
    ToolManager toolManager
) : Tool, IInputProcessor, ILoadableSingleton
{
#nullable disable
    AreaBlockObjectPicker areaPicker;
    BlockObjectSelectionDrawer selectionDrawer;
#nullable enable

    public MacroManagementSelectionFlags FilterFlag { get; set; }
    public PrefabSpec? SelectingPrefab
    {
        get;
        set
        {
            field = value;
            selectingPrefabName = value?.PrefabName;
        }
    }
    string? selectingPrefabName;

    public void Load()
    {
        areaPicker = areaBlockObjectPickerFactory.CreatePickingUpwards();
        selectionDrawer = blockObjectSelectionDrawerFactory.Create(BrushColors.Positive, BrushColors.Positive, BrushColors.Neutral);
    }

    public override void Enter()
    {
        inputService.AddInputProcessor(this);
        cursorService.SetCursor("GrabbingCursor");
    }

    public override void Exit()
    {
        areaPicker.Reset();
        StopDrawing();
        inputService.RemoveInputProcessor(this);
        cursorService.ResetCursor();
    }

    public bool ProcessInput()
        => areaPicker.PickBlockObjects<PrefabSpec>(PreviewCallback, ActionCallback, ShowNoneCallback);

    void PreviewCallback(IEnumerable<BlockObject> blockObjects, Vector3Int start, Vector3Int end, bool selectionStarted, bool selectingArea)
    {
        selectionDrawer.Draw(FilterObjects(blockObjects), start, end, selectingArea);
    }

    void ActionCallback(IEnumerable<BlockObject> blockObjects, Vector3Int start, Vector3Int end, bool selectionStarted, bool selectingArea)
    {
        if (!SelectingPrefab) { return; }

        ShowNoneCallback();
        toolManager.SwitchToDefaultTool();

        var objs = FilterObjects(blockObjects)
            .Select(q => q.GetComponentFast<PrefabSpec>())
            .ToImmutableArray();
        multiSelectService.SelectItems(SelectingPrefab, objs);
    }

    void ShowNoneCallback() => StopDrawing();

    void StopDrawing()
    {
        selectionDrawer.StopDrawing();
    }

    IEnumerable<BlockObject> FilterObjects(IEnumerable<BlockObject> objs)
    {
        foreach (var obj in objs)
        {
            var prefab = obj.GetComponentFast<PrefabSpec>();

            if (!prefab || prefab.Name != selectingPrefabName) { continue; }
            if (!multiSelectService.MatchPausable(obj, FilterFlag)) { continue; }

            yield return obj;
        }
    }

}