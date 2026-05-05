namespace TimberUi.Tools;

public readonly record struct SelectEntityToolOptions(
    BaseComponent? Source = null,
    string? Cursor = null,
    Color? HighlightColor = null,
    Func<SelectableObject, bool>? Filter = null
);

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class SelectEntityTool(
    ToolService toolService,
    CursorService cursorService,
    InputService inputService,
    RollingHighlighter highlighter,
    SelectableObjectRaycaster selectableObjectRaycaster,
    EntitySelectionService entitySelectionService
) : ITool, IConstructionModeEnabler, IInputProcessor
{
    public const string DefaultCursor = "PickObjectCursor";
    public static readonly Color DefaultHighlightColor = TimberUiUtils.SuccessColor;

    SelectEntityToolOptions options;
    TaskCompletionSource<EntityComponent?>? tcs;

    public void Enter()
    {
        inputService.AddInputProcessor(this);
        cursorService.SetCursor(options.Cursor ?? DefaultCursor);
    }

    public void Exit()
    {
        tcs?.TrySetResult(null);
        tcs = null;

        inputService.RemoveInputProcessor(this);
        cursorService.ResetCursor();
        highlighter.UnhighlightAllPrimary();

        if (options.Source)
        {
            entitySelectionService.Select(options.Source);
        }
        options = default;
    }

    void SetResult(BaseComponent comp)
    {
        tcs?.TrySetResult(comp.GetComponent<EntityComponent>());
        tcs = null;
        toolService.SwitchToDefaultTool();
    }

    public bool ProcessInput()
    {
        if (tcs is null)
        {
            toolService.SwitchToDefaultTool();
            return true;
        }

        if (selectableObjectRaycaster.TryHitSelectableObject(out var hitObj)
            && IsValid(hitObj))
        {
            if (inputService.MainMouseButtonDown && !inputService.MouseOverUI)
            {
                SetResult(hitObj);
                return true;
            }
            else
            {
                highlighter.HighlightPrimary(hitObj, options.HighlightColor ?? DefaultHighlightColor);
            }
        }
        else
        {
            highlighter.UnhighlightAllPrimary();
        }

        return false;
    }

    bool IsValid(SelectableObject obj) => options.Filter is null || options.Filter(obj);

    public async Task<EntityComponent?> SelectAsync() => await SelectAsync(default);

    public async Task<EntityComponent?> SelectAsync(SelectEntityToolOptions options)
    {
        tcs?.TrySetResult(null);

        this.options = options;
        var t = tcs = new();
        toolService.SwitchTool(this);

        return await t.Task;
    }

}
