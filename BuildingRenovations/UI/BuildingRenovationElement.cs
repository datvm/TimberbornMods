namespace BuildingRenovations.UI;

[BindTransient]
public class BuildingRenovationElement(
    ILoc t,
    IGoodService goodService,
    DevModeManager devModeManager,
    PriorityToggleGroupFactory priorityToggleGroupFactory,
    BuilderPrioritySpriteLoader builderPrioritySpriteLoader,
    DialogBoxShower dialogBoxShower
) : VisualElement, IPrioritizable
{
#nullable disable
    Button btnFinishNow;
    Label lblName, lblProgress, lblMaterials;
    ProgressBar pgbProgress;
    PriorityToggleGroup priorityToggleGroup;
    VisualElement materialPanel;
#nullable enable

    public BuildingRenovationComponent? Component { get; private set; }
    public Priority Priority => Component?.Priority ?? Priority.Normal;
    bool priorityControlEnabled;

    public BuildingRenovationElement Init()
    {
        this.SetDisplay(false);

        lblName = this.AddGameLabel();
        pgbProgress = this.AddProgressBar().SetMarginBottom();
        lblProgress = pgbProgress.AddProgressLabel();

        materialPanel = this.AddChild().SetMarginBottom();
        var priorityContainer = materialPanel.AddChild().SetMarginBottom(5);
        priorityToggleGroup = priorityToggleGroupFactory.CreatePriorityToggle(
            priorityContainer,
            builderPrioritySpriteLoader,
            "LV.BRe.RenoPriorityShort");
        lblMaterials = materialPanel.AddGameLabel().SetMarginBottom();

        this.AddStretchedEntityFragmentButton(
            t.T("LV.BRe.CancelReno"),
            onClick: ConfirmCancel,
            color: EntityFragmentButtonColor.Red);
        btnFinishNow = this.AddGameButton("Finish Now (Dev)", onClick: FinishNow)
            .SetMargin(top: 10)
            .SetDisplay(false);

        return this;
    }

    public void SetComponent(BuildingRenovationComponent? component)
    {
        if (!component)
        {
            priorityToggleGroup.Disable();
            Unset();
            return;
        }

        Component = component;
        btnFinishNow.SetDisplay(devModeManager.Enabled);
    }

    public void Update()
    {
        if (!Component) { return; }

        var spec = Component!.CurrentSpec;
        if (spec is null || Component.CurrentId is null)
        {
            this.SetDisplay(false);
            return;
        }

        lblName.text = t.T("LV.BRe.CurrentRenovation", spec.Title.Value);

        if (Component.IsWorking)
        {
            var work = Component.Work;
            pgbProgress.SetProgress(
                work.Progress,
                lblProgress,
                t.TDays(work.DaysLeft));
            ToggleMaterialPanel(false);
        }
        else
        {
            pgbProgress.SetProgress(0, lblProgress, t.T("LV.BRe.WaitingForMaterial"));
            lblMaterials.text = GetMaterialProgressText(spec);
            ToggleMaterialPanel(true);
            priorityToggleGroup.UpdateGroup();
        }

        this.SetDisplay(true);
    }

    string GetMaterialProgressText(RenovationSpec spec)
    {
        var receiver = Component!.Distro;
        var stored = receiver.StoredGoods;
        var remaining = receiver.RemainingDemand;
        StringBuilder str = new();

        foreach (var c in spec.Cost.Where(q => q.Amount > 0))
        {
            var have = stored.GetValueOrDefault(c.Id);
            var left = remaining.GetValueOrDefault(c.Id);
            var total = have + left;
            if (total <= 0) { total = c.Amount; }

            var g = goodService.GetGood(c.Id);
            str.AppendLine($"● {have}/{total} {g.PluralDisplayName.Value}");
        }

        return str.ToString();
    }

    public void Unset()
    {
        Component = null;
        this.SetDisplay(false);
        ToggleMaterialPanel(false);
    }

    void ToggleMaterialPanel(bool enabled)
    {
        materialPanel.SetDisplay(enabled);
        if (priorityControlEnabled == enabled) { return; }

        priorityControlEnabled = enabled;
        if (enabled)
        {
            priorityToggleGroup.Enable(this);
        }
        else
        {
            priorityToggleGroup.Disable();
        }
    }

    void ConfirmCancel()
    {
        if (Component?.CurrentId is null) { return; }

        dialogBoxShower.Create()
            .SetLocalizedMessage("LV.BRe.CancelRenoConfirm")
            .SetConfirmButton(TimberUiUtils.DoNothing, t.T("LV.BRe.CancelNo"))
            .SetCancelButton(() => Component?.CancelCurrentRenovation(), t.T("LV.BRe.CancelRenoYes"))
            .Show();
    }

    void FinishNow() => Component?.FinishNow();

    public void SetPriority(Priority priority)
    {
        if (Component is null || !Component.CanChangePriority) { return; }
        Component.ChangePriority(priority);
    }
}
