namespace CraneHeads.Services;

[BindSingleton]
public class TrebuchetTargetTool(
    InputService input,
    CursorCoordinatesPicker cursor,
    ToolService tools,
    EntitySelectionService selection,
    CursorService cursors,
    UISoundController sounds,
    ILoc t,
    TrebuchetTrajectoryService trajectory,
    TrebuchetShotPreview preview,
    TrebuchetPreviewTooltip tooltip
) : ITool, IToolDescriptor, IInputProcessor
{
    readonly List<Vector3> path = [];
    readonly List<Vector3Int> blockers = [];
    CraneHeadTrebuchetLauncher? launcher;
    CraneHeadTrebuchetModel? model;

    public void Enter()
    {
        input.AddInputProcessor(this);
        cursors.SetCursor("PickObjectCursor");
    }

    public void Exit()
    {
        input.RemoveInputProcessor(this);
        cursors.ResetCursor();
        preview.Hide();
        tooltip.Hide();
        model?.SetPreview(null);
        if (launcher)
        {
            selection.Select(launcher);
        }

        launcher = null;
        model = null;
    }

    public ToolDescription DescribeTool()
        => new ToolDescription.Builder().AddPrioritizedSection(t.T("LV.CrH.PickTarget")).Build();

    public void Begin(CraneHeadTrebuchetLauncher next)
    {
        launcher = next;
        model = next.GetComponent<CraneHeadTrebuchetModel>();
        tools.SwitchTool(this);
    }

    public bool ProcessInput()
    {
        if (launcher is not { } shot || !shot)
        {
            return false;
        }

        var trebuchet = shot.GetComponent<CraneHeadTrebuchet>();
        var picked = cursor.Pick();
        if (picked is null || !trebuchet)
        {
            preview.Hide();
            tooltip.Hide();
            model?.SetPreview(null);
            return false;
        }

        var dest = picked.Value.TileCoordinates;
        var check = shot.Evaluate(dest);
        model?.SetPreview(dest);
        trajectory.FillWorldPath(trebuchet.Origin, dest, trebuchet.PeakDelta, path);
        trajectory.FillBlockingCells(trebuchet.Origin, dest, trebuchet.PeakDelta, shot.GetComponent<BlockObject>(), blockers);
        preview.Show(dest, check.IsValid, path, blockers);
        tooltip.Show(check);

        if (!input.MainMouseButtonDown || input.MouseOverUI)
        {
            return false;
        }

        if (!check.IsValid)
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
