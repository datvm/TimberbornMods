namespace CraneHeads.Components;

[AddTemplateModule2(typeof(CraneHeadTrebuchetSpec))]
public class CraneHeadTrebuchetIssueStatus(
    ILoc t
) : BaseComponent, IAwakableComponent, IFinishedStateListener, IInitializableEntity, IDeletableEntity, ISelectionListener
{
    const string Sprite = "TrebuchetBlocked";

    CraneHeadTrebuchet trebuchet = null!;
    CraneHeadTrebuchetInventory inventory = null!;
    CraneHeadTrebuchetLauncher launcher = null!;
    BlockObject bo = null!;
    StatusToggle overweight = null!;
    StatusToggle needPayload = null!;
    StatusToggle needTarget = null!;
    StatusToggle blocked = null!;
    StatusToggle outOfRange = null!;
    StatusToggle tooHigh = null!;
    StatusToggle badLanding = null!;
    StatusToggle sameTile = null!;
    StatusToggle? active;

    public void Awake()
    {
        trebuchet = GetComponent<CraneHeadTrebuchet>();
        inventory = GetComponent<CraneHeadTrebuchetInventory>();
        launcher = GetComponent<CraneHeadTrebuchetLauncher>();
        bo = GetComponent<BlockObject>();
        overweight = Issue("LV.CrH.TrebuchetOverweight", "LV.CrH.StatusOverweightShort");
        needPayload = Issue("LV.CrH.TrebuchetNeedPayload", "LV.CrH.StatusNeedPayloadShort");
        needTarget = Issue("LV.CrH.TrebuchetNeedTarget", "LV.CrH.StatusNeedTargetShort");
        blocked = Issue("LV.CrH.TrebuchetBlocked", "LV.CrH.StatusBlockedShort");
        outOfRange = Issue("LV.CrH.TrebuchetOutOfRange", "LV.CrH.StatusOutOfRangeShort");
        tooHigh = Issue("LV.CrH.TrebuchetTooHigh", "LV.CrH.StatusTooHighShort");
        badLanding = Issue("LV.CrH.TrebuchetBadLanding", "LV.CrH.StatusBadLandingShort");
        sameTile = Issue("LV.CrH.TrebuchetSameTile", "LV.CrH.StatusSameTileShort");
        GetComponent<StatusSubject>().RegisterStatuses(
            [overweight, needPayload, needTarget, blocked, outOfRange, tooHigh, badLanding, sameTile]);
    }

    public void InitializeEntity()
    {
        launcher.TargetChanged += OnIssueChanged;
        launcher.TrajectoryChecked += OnIssueChanged;
        inventory.Changed += OnIssueChanged;
    }

    public void DeleteEntity()
    {
        launcher.TargetChanged -= OnIssueChanged;
        launcher.TrajectoryChecked -= OnIssueChanged;
        inventory.Changed -= OnIssueChanged;
    }

    public void OnEnterFinishedState() => Refresh();

    public void OnExitFinishedState() => Show(null);

    public void OnSelect()
    {
        if (!bo.IsPreview)
        {
            Refresh();
        }
    }

    public void OnUnselect()
    {
    }

    void OnIssueChanged(object sender, EventArgs e) => Refresh();

    void Refresh()
    {
        if (!trebuchet.IsFinished)
        {
            Show(null);
            return;
        }

        Show(CurrentIssue());
    }

    StatusToggle? CurrentIssue()
    {
        if (launcher.Target is { } dest)
        {
            var check = launcher.Evaluate(dest);
            if (!check.IsValid)
            {
                return check.Status switch
                {
                    TrebuchetShotStatus.OutOfRange => outOfRange,
                    TrebuchetShotStatus.TooHigh => tooHigh,
                    TrebuchetShotStatus.InvalidLanding => badLanding,
                    TrebuchetShotStatus.SameTile => sameTile,
                    _ => blocked,
                };
            }
        }

        if (inventory.IsOverweight)
        {
            return overweight;
        }

        if (inventory.Requested.Count == 0)
        {
            return needPayload;
        }

        if (launcher.Target is null)
        {
            return needTarget;
        }

        return null;
    }

    void Show(StatusToggle? next)
    {
        if (active == next)
        {
            return;
        }

        active?.Deactivate();
        active = next;
        active?.Activate();
    }

    StatusToggle Issue(string description, string alert, params object[] args)
        => StatusToggle.CreateNormalStatusWithAlertAndFloatingIcon(
            Sprite,
            t.T(description, args),
            t.T(alert));
}
