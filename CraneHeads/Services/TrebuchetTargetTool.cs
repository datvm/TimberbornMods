namespace CraneHeads.Services;

[BindSingleton]
public class TrebuchetTargetTool(
    InputService input,
    CursorCoordinatesPicker cursor,
    ToolService tools,
    EntitySelectionService selection,
    CursorService cursors,
    TerrainHighlightingService highlighter,
    UISoundController sounds,
    ILoc t,
    TrebuchetTrajectoryService trajectory
) : ITool, IToolDescriptor, IInputProcessor
{
    readonly List<Vector3Int> preview = [];
    CraneHeadTrebuchet? trebuchet;

    public void Enter()
    {
        input.AddInputProcessor(this);
        cursors.SetCursor("PickObjectCursor");
    }

    public void Exit()
    {
        input.RemoveInputProcessor(this);
        cursors.ResetCursor();
        highlighter.ClearHighlight();
        if (trebuchet)
        {
            selection.Select(trebuchet);
        }

        trebuchet = null;
    }

    public ToolDescription DescribeTool()
        => new ToolDescription.Builder().AddPrioritizedSection(t.T("LV.CrH.PickTarget")).Build();

    public void Begin(CraneHeadTrebuchet next)
    {
        trebuchet = next;
        tools.SwitchTool(this);
    }

    public bool ProcessInput()
    {
        if (trebuchet is not { } shot || !shot)
        {
            return false;
        }

        var picked = cursor.Pick();
        if (picked is null)
        {
            highlighter.ClearHighlight();
            return false;
        }

        var dest = picked.Value.TileCoordinates;
        var valid = shot.InRange(dest)
            && dest.z <= shot.Origin.z + shot.PeakDelta
            && trajectory.IsPathClear(shot.Origin, dest, shot.PeakDelta, shot.GetComponent<BlockObject>());

        if (trajectory.TryGetPath(shot.Origin, dest, shot.PeakDelta, preview))
        {
            highlighter.UpdateHighlight(preview);
        }
        else
        {
            highlighter.ClearHighlight();
        }

        if (!input.MainMouseButtonDown || input.MouseOverUI)
        {
            return false;
        }

        if (!valid)
        {
            sounds.PlayCantDoSound();
            return true;
        }

        shot.SetTarget(dest);
        sounds.PlayClickSound();
        tools.SwitchToDefaultTool();
        return true;
    }
}
