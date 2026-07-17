namespace PowerLines.Services;

[BindSingleton]
public class PowerConnectionTool(
    InputService inputService,
    SelectableObjectRaycaster selectableObjectRaycaster,
    EntitySelectionService entitySelectionService,
    ToolService toolService,
    PowerLineConnectionService connectionService,
    CursorService cursorService,
    ILoc loc,
    UISoundController uiSoundController,
    Highlighter highlighter,
    PowerLineRenderer renderer,
    PowerLinePreviewTooltip previewTooltip
) : ITool, IToolDescriptor, IInputProcessor, IConstructionModeEnabler
{
    PowerLineComponent? current;

    public void SwitchTo(PowerLineComponent powerLine)
    {
        current = powerLine;
    }

    public void Enter()
    {
        Asserts.FieldIsNotNull(this, current, nameof(current));
        inputService.AddInputProcessor(this);
        cursorService.SetCursor("PickObjectCursor");
        highlighter.HighlightPrimary(current, TimberUiUtils.NeutralColor);
        renderer.StartRenderingConnectionTool();
    }

    public void Exit()
    {
        inputService.RemoveInputProcessor(this);
        cursorService.ResetCursor();
        highlighter.UnhighlightAllPrimary();
        highlighter.UnhighlightAllSecondary();
        renderer.StopRenderingConnectionTool();
        previewTooltip.Hide();

        if (current)
        {
            entitySelectionService.Select(current);
        }

        current = null;
    }

    public ToolDescription DescribeTool()
        => new ToolDescription.Builder()
            .AddPrioritizedSection(loc.T("LV.PL.PickDestination"))
            .Build();

    public bool ProcessInput()
    {
        if (current is not { } from) { return false; }

        var other = selectableObjectRaycaster.TryHitSelectableObject(out var hitObject)
            ? hitObject.GetComponent<PowerLineComponent>()
            : null;

        highlighter.UnhighlightAllSecondary();

        if (other is not { } hovered || hovered == from)
        {
            renderer.UpdateToolPreview(from, null, false);
            previewTooltip.Hide();
        }
        else
        {
            var check = connectionService.EvaluateConnection(from, hovered);
            renderer.UpdateToolPreview(from, hovered, check.CanConnect);
            previewTooltip.Show(check);

            highlighter.HighlightSecondary(
                hovered,
                check.CanConnect ? TimberUiUtils.SuccessColor : TimberUiUtils.DangerColor);

            if (inputService.MainMouseButtonDown && !inputService.MouseOverUI)
            {
                if (check.CanConnect)
                {
                    Connect(from, hovered);
                    uiSoundController.PlayClickSound();
                    return true;
                }

                uiSoundController.PlayCantDoSound();
                return false;
            }

            return false;
        }

        if (inputService.MainMouseButtonDown && !inputService.MouseOverUI)
        {
            uiSoundController.PlayCantDoSound();
        }

        return false;
    }

    void Connect(PowerLineComponent from, PowerLineComponent other)
    {
        connectionService.Connect(from, other);
        toolService.SwitchToDefaultTool();
        entitySelectionService.Select(other);

        // Chain connections from the destination when it still has free slots
        // (same flow as ZiplineConnectionAddingTool).
        if (connectionService.HasFreeSlots(other))
        {
            SwitchTo(other);
            toolService.SwitchTool(this);
        }
    }
}
