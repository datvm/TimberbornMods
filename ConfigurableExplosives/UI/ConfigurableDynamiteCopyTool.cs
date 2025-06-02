namespace ConfigurableExplosives.UI;

public class ConfigurableDynamiteCopyTool(
    AreaBlockObjectPickerFactory _areaBlockObjectPickerFactory,
    InputService inputService,
    BlockObjectSelectionDrawerFactory _blockObjectSelectionDrawerFactory
) : IInputProcessor, ILoadableSingleton
{
    static readonly Color HighlightColor = new(.2f, .2f, .2f);
    static readonly Color TileColor = new(.8f, .8f, .8f);
    static readonly Color SideColor = new(1f, 1f, 1f);

    Action<ImmutableArray<ConfigurableDynamiteComponent>>? callback;

    AreaBlockObjectPicker _areaBlockObjectPicker = null!;
    BlockObjectSelectionDrawer _highlightSelectionDrawer = null!;
    BlockObjectSelectionDrawer _actionSelectionDrawer = null!;

    public void Load()
    {
        _areaBlockObjectPicker = _areaBlockObjectPickerFactory.CreatePickingDownwards();
        _highlightSelectionDrawer = _blockObjectSelectionDrawerFactory.Create(HighlightColor, TileColor, SideColor);
        _actionSelectionDrawer = _blockObjectSelectionDrawerFactory.Create(HighlightColor, TileColor, SideColor);
    }

    public void Activate(Action<ImmutableArray<ConfigurableDynamiteComponent>> callback)
    {
        this.callback = callback;
        inputService.AddInputProcessor(this);
    }

    public void Cancel()
    {
        callback = null;
        inputService.RemoveInputProcessor(this);
    }

    public bool ProcessInput()
    {
        if (callback is null) // Should not happen but who knows
        {
            Cancel();
            return false;
        }

        return _areaBlockObjectPicker.PickBlockObjects<ConfigurableDynamiteComponent>(PreviewCallback, ActionCallback, ShowNoneCallback);
    }

    private void PreviewCallback(IEnumerable<BlockObject> blockObjects, Vector3Int start, Vector3Int end, bool selectionStarted, bool selectingArea)
    {
        IEnumerable<BlockObject> blockObjects2 = blockObjects.Where((BlockObject bo) => bo.GetComponentFast<ConfigurableDynamiteComponent>()?.enabled ?? false);
        if (selectionStarted && !selectingArea)
        {
            _actionSelectionDrawer.Draw(blockObjects2, start, end, selectingArea: false);
        }
        else if (selectingArea)
        {
            _actionSelectionDrawer.Draw(blockObjects2, start, end, selectingArea: true);
        }
        else
        {
            _highlightSelectionDrawer.Draw(blockObjects2, start, end, selectingArea: false);
        }
    }

    private void ActionCallback(IEnumerable<BlockObject> blockObjects, Vector3Int start, Vector3Int end, bool selectionStarted, bool selectingArea)
    {
        if (callback is null) { return; }

        var dynamites = blockObjects
            .Select(q => q.GetComponentFast<ConfigurableDynamiteComponent>())
            .Where(q => q && q.enabled)
            .ToImmutableArray();

        callback(dynamites);
        
        ClearHighlights();
        Cancel();
    }

    private void ShowNoneCallback()
    {
        ClearHighlights();
    }

    void ClearHighlights()
    {
        _highlightSelectionDrawer.StopDrawing();
        _actionSelectionDrawer.StopDrawing();
    }

}
