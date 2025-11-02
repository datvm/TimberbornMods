namespace TImprove4UX.Services;

public class BuildingCopyService(
    InputService inputService,
    EventBus eb,
    SelectableObjectRaycaster selectableObjectRaycaster,
    ToolManager toolManager,
    ToolButtonService toolButtons,
    DevModeManager devModeManager
) : ILoadableSingleton, IPostLoadableSingleton, IInputProcessor
{
    const string KeyId = "CopyBuilding";

    FrozenDictionary<string, Tool> toolByPrefabName = FrozenDictionary<string, Tool>.Empty;

    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnToolEntered(ToolEnteredEvent e)
    {
        if (e.Tool is CursorTool)
        {
            inputService.AddInputProcessor(this);
        }
    }

    [OnEvent]
    public void OnToolExited(ToolExitedEvent _)
    {
        inputService.RemoveInputProcessor(this);
    }

    public void PostLoad()
    {
        List<KeyValuePair<string, Tool>> tools = [];

        foreach (var tb in toolButtons.ToolButtons)
        {
            if (tb.Tool is not BlockObjectTool t) { continue; }

            var prefab = t.Prefab.GetComponentFast<PrefabSpec>();
            if (!prefab) { continue; }

            tools.Add(new(prefab.name, t));
        }

        toolByPrefabName = tools.ToFrozenDictionary();
    }

    public bool ProcessInput()
    {
        if (inputService.IsKeyDown(KeyId))
        {
            TryCopying();
            return true;
        }

        return false;
    }

    void TryCopying()
    {
        if (!selectableObjectRaycaster.TryHitSelectableObject(out var obj) || !obj) { return; }

        var prefab = obj.GetComponentFast<PrefabSpec>();
        if (!prefab || !toolByPrefabName.TryGetValue(prefab.Name, out var tool)) { return; }

        if (tool.DevModeTool && !devModeManager.Enabled) { return; }

        toolManager.SwitchTool(tool);
    }

}
