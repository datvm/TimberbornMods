namespace SuperCursor.Services;

public class SuperCursorTool(EntitySelectionService entitySelectionService, InputService inputService, IOptionsBox optionsBox, UIVisibilityManager uiVisibilityManager, SelectableObjectRaycaster selectableObjectRaycaster,
    SuperCursorText label,
    IEnumerable<IObjectDescriber> objDescribers,
    CursorCoordinatesPicker cursorCoords,
    IEnumerable<ICoordDescriber> coordDescribers,
    ILoc loc
) : CursorTool(entitySelectionService, inputService, optionsBox, uiVisibilityManager, selectableObjectRaycaster), IInputProcessor
{
    readonly SelectableObjectRaycaster selectableObjectRaycaster = selectableObjectRaycaster;

    static readonly Vector2 MouseMargin = new(10, 0);
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

        var mousePos = _inputService.MousePosition;
        var x = mousePos.x + MouseMargin.x;
        // y is upside-down between the game and the UI
        var y = Screen.height - mousePos.y + MouseMargin.y;

        label.MoveTo(x, y);
        label.Visible = true;
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
        var coords = cursorCoords.CursorCoordinates();
        if (coords is null) { return; }

        foreach (var desc in coordDescribers)
        {
            desc.Describe(builder, coords.Value);
        }
    }

    public override void Exit()
    {
        label.Visible = false;
        base.Exit();
    }

    bool IInputProcessor.ProcessInput()
    {
        var result = ProcessInput();

        if (Keyboard.current.shiftKey.isPressed)
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
