namespace SuperCursor.Services;

public class SuperCursorTool(EntitySelectionService entitySelectionService, InputService inputService, IOptionsBox optionsBox, UIVisibilityManager uiVisibilityManager, SelectableObjectRaycaster selectableObjectRaycaster,
    SuperCursorText label,
    IEnumerable<IObjectDescriber> objDescribers,
    CursorCoordinatesPicker cursorCoords,
    IEnumerable<ICoordDescriber> coordDescribers,
    ILoc loc,
    EventBus eb
) : CursorTool(entitySelectionService, inputService, optionsBox, uiVisibilityManager, selectableObjectRaycaster), IInputProcessor, ILoadableSingleton
{
    const string Keybinding = "SuperCursor";
    readonly SelectableObjectRaycaster selectableObjectRaycaster = selectableObjectRaycaster;
    readonly InputService inputService = inputService;

    static readonly Vector2 MouseMargin = new(10, 10);

    public void Load()
    {
        eb.Register(this);
    }

    void ProcessInfo()
    {
        StringBuilder info = new();

        ProcessCoords(info);
        ProcessObject(info);

        if (info.Length == 0)
        {
            info.Append(loc.T("LV.SC.Nothing"));
        }

        label.Text = info.ToString();

        MoveLabel();
        label.Visible = true;
    }

    void MoveLabel()
    {
        var mousePos = _inputService.MousePosition;

        var x = mousePos.x + MouseMargin.x;
        // y is upside-down between the game and the UI
        var y = Screen.height - mousePos.y + MouseMargin.y;

        var scale = label.panel.scaledPixelsPerPoint;

        var scaledX = x / scale;
        var scaledY = y / scale;

        scaledX = Math.Min(scaledX, Screen.width / scale - label.layout.width);
        scaledY = Math.Min(scaledY, Screen.height / scale - label.layout.height);

        label.MoveTo(scaledX, scaledY);
    }

    void ProcessObject(StringBuilder builder)
    {
        if (!selectableObjectRaycaster.TryHitSelectableObject(out var obj)) { return; }

        foreach (var desc in objDescribers)
        {
            desc.Describe(builder, obj);
        }
    }

    void ProcessCoords(StringBuilder builder)
    {
        var coords = cursorCoords.Pick();
        if (coords is null) { return; }

        foreach (var desc in coordDescribers)
        {
            desc.Describe(builder, coords.Value);
        }
    }

    [OnEvent]
    public void OnToolExited(ToolExitedEvent e)
    {
        if (e.Tool == this)
        {
            label.Visible = false;
        }
    }

    bool IInputProcessor.ProcessInput()
    {
        var result = ProcessInput();

        if (!inputService.MouseOverUI && inputService.IsKeyHeld(Keybinding))
        {
            ProcessInfo();
        }
        else
        {
            label.Visible = false;
        }

        return result;
    }

}
