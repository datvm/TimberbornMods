namespace PowerCopy.Services;

public class PowerCopyAreaTool(
    AreaBlockObjectPickerFactory pickerFac,
    ToolService toolService,
    InputService inputService,
    BlockObjectSelectionDrawerFactory drawerFac,
    ISpecService specs,
    CursorService cursorService,
    ObjectListingService objectListingService,
    EntitySelectionService entitySelectionService
) : ITool, ILoadableSingleton, IInputProcessor
{

#nullable disable
    AreaBlockObjectPicker picker;
    BlockObjectSelectionDrawer drawer;
#nullable enable

    ObjectListingQuery query;

    TaskCompletionSource<EntityComponent[]>? tcs;

    public void Load()
    {
        picker = pickerFac.CreatePickingUpwards();

        var areaSpec = specs.GetSingleSpec<BuilderPriorityToolSpec>();
        var dupSpec = specs.GetSingleSpec<DuplicationSystemColorsSpec>();
        drawer = drawerFac.Create(dupSpec.TargetColor, areaSpec.PriorityTileColor, areaSpec.PrioritySideColor);
    }

    public async Task<EntityComponent[]> Pick(ObjectListingQuery query)
    {
        CancelIfPending();

        this.query = query;
        var curr = tcs = new();

        toolService.SwitchTool(this);

        return await curr.Task;
    }

    public void Enter()
    {
        inputService.AddInputProcessor(this);
        cursorService.SetCursor(DuplicateSettingsTool.CursorKey);
    }

    public void Exit()
    {
        inputService.RemoveInputProcessor(this);
        cursorService.ResetCursor();
        picker.Reset();
        drawer.StopDrawing();
        CancelIfPending();
    }

    void CancelIfPending()
    {
        if (query != default && query.Source)
        {
            entitySelectionService.Select(query.Source);
            query = default;
        }

        if (tcs is null) { return; }

        tcs.TrySetResult([]);
        tcs = null;
    }

    public bool ProcessInput()
        => picker.PickBlockObjects<EntityComponent>(PreviewCallback, ActionCallback, ShowNoneCallback);

    void PreviewCallback(IEnumerable<BlockObject> blockObjects, Vector3Int start, Vector3Int end, bool selectionStarted, bool selectingArea)
    {
        drawer.Draw(blockObjects.Where(Filter), start, end, selectingArea);
    }

    void ActionCallback(IEnumerable<BlockObject> blockObjects, Vector3Int start, Vector3Int end, bool selectionStarted, bool selectingArea)
    {
        var prev = query.Source;
        if (prev)
        {
            entitySelectionService.Select(prev);
        }

        if (tcs is not null)
        {
            tcs.TrySetResult([.. blockObjects
                .Where(Filter)
                .Select(bo => bo.GetComponent<EntityComponent>())
                .Where(c => c)]);
            tcs = null;
        }

        query = default;

        toolService.SwitchToDefaultTool();
    }

    void ShowNoneCallback()
    {
        drawer.StopDrawing();
    }

    bool Filter(BlockObject obj) => objectListingService.CanCopy(query, obj);

}
