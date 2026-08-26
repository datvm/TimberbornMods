namespace CraneHeads.Components;

[AddTemplateModule2(typeof(CraneHeadTrebuchetSpec))]
public class CraneHeadTrebuchetSelectionPreview(
    TrebuchetTrajectoryService trajectory,
    TrebuchetShotPreview preview
) : BaseComponent, IAwakableComponent, IUpdatableComponent, ISelectionListener
{
    readonly List<Vector3> path = [];
    readonly List<Vector3Int> blockers = [];
    CraneHeadTrebuchet trebuchet = null!;
    CraneHeadTrebuchetLauncher launcher = null!;
    BlockObject bo = null!;

    public void Awake()
    {
        trebuchet = GetComponent<CraneHeadTrebuchet>();
        launcher = GetComponent<CraneHeadTrebuchetLauncher>();
        bo = GetComponent<BlockObject>();
        DisableComponent();
    }

    public void Update()
    {
        if (!trebuchet.IsFinished || launcher.Target is not { } dest)
        {
            preview.Hide();
            return;
        }

        var check = launcher.Evaluate(dest);
        trajectory.FillWorldPath(trebuchet.Origin, dest, trebuchet.PeakDelta, path);
        trajectory.FillBlockingCells(trebuchet.Origin, dest, trebuchet.PeakDelta, bo, blockers);
        preview.Show(dest, check.IsValid, path, blockers);
    }

    public void OnSelect()
    {
        if (bo.IsPreview)
        {
            return;
        }

        EnableComponent();
    }

    public void OnUnselect()
    {
        DisableComponent();
        preview.Hide();
    }
}
