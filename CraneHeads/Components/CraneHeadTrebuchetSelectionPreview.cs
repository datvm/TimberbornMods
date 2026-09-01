namespace CraneHeads.Components;

[AddTemplateModule2(typeof(CraneHeadTrebuchetSpec))]
public class CraneHeadTrebuchetSelectionPreview(
    TrebuchetTrajectoryService trajectory,
    TrebuchetShotPreview preview
) : BaseComponent, IAwakableComponent, IUpdatableComponent, ISelectionListener, IInitializableEntity, IDeletableEntity
{
    readonly List<Vector3> path = [];
    readonly List<Vector3Int> blockers = [];
    CraneHeadTrebuchet trebuchet = null!;
    CraneHeadTrebuchetLauncher launcher = null!;
    BlockObject bo = null!;
    bool selected;

    public void Awake()
    {
        trebuchet = GetComponent<CraneHeadTrebuchet>();
        launcher = GetComponent<CraneHeadTrebuchetLauncher>();
        bo = GetComponent<BlockObject>();
        DisableComponent();
    }

    public void InitializeEntity()
    {
        launcher.TargetChanged += OnTargetChanged;
        launcher.TrajectoryChecked += OnTargetChanged;
    }

    public void DeleteEntity()
    {
        launcher.TargetChanged -= OnTargetChanged;
        launcher.TrajectoryChecked -= OnTargetChanged;
    }

    public void Update()
    {
        if (selected)
        {
            preview.Draw();
        }
    }

    public void OnSelect()
    {
        if (bo.IsPreview)
        {
            return;
        }

        selected = true;
        EnableComponent();
        Rebuild();
    }

    public void OnUnselect()
    {
        selected = false;
        DisableComponent();
        preview.Hide();
    }

    void OnTargetChanged(object sender, EventArgs e)
    {
        if (selected)
        {
            Rebuild();
        }
    }

    void Rebuild()
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
}
