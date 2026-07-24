namespace BuildingRenovations.UI;

[BindTransient]
public class BuildingRenovationElement(
    ILoc t,
    DevModeManager devModeManager,
    PriorityToggleGroupFactory priorityToggleGroupFactory,
    BuilderPrioritySpriteLoader builderPrioritySpriteLoader,
    DialogBoxShower dialogBoxShower,
    IContainer container
) : VisualElement, IPrioritizable
{
#nullable disable
    Button btnFinishNow;
    Label lblName, lblProgress;
    ProgressBar pgbProgress;
    PriorityToggleGroup priorityToggleGroup;
    VisualElement materialPanel, materialRowsContainer;
#nullable enable

    readonly List<BuildingRenovationMaterialRow> materialRows = [];

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
        materialRowsContainer = materialPanel.AddChild();

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

        var renovation = Component!.CurrentRenovation;
        if (renovation is null)
        {
            this.SetDisplay(false);
            return;
        }

        lblName.text = t.T("LV.BRe.CurrentRenovation", renovation.Name);

        if (Component.IsWorking)
        {
            var work = Component.Work;
            pgbProgress.SetProgress(
                work.Progress,
                lblProgress,
                t.TDaysOrHours(work.DaysLeft));
            ToggleMaterialPanel(false);
        }
        else
        {
            pgbProgress.SetProgress(0, lblProgress, t.T("LV.BRe.WaitingForMaterial"));
            PopulateMaterials(renovation.Cost);
            ToggleMaterialPanel(true);
            priorityToggleGroup.UpdateGroup();
        }

        this.SetDisplay(true);
    }

    void PopulateMaterials(IEnumerable<GoodAmount> cost)
    {
        var receiver = Component!.Distro;
        var stored = receiver.StoredGoods;

        var index = 0;
        foreach (var item in cost)
        {
            while (index >= materialRows.Count)
            {
                var newRow = materialRowsContainer.AddChild(container.GetInstance<BuildingRenovationMaterialRow>);
                newRow.OnSciencePayClicked += OnSciencePaymentRequested;
                materialRows.Add(newRow);
            }
            var row = materialRows[index];

            var paid = stored.GetValueOrDefault(item.GoodId);
            row.SetContent(item.GoodId, paid, item.Amount);

            index++;
        }

        for (; index < materialRows.Count; index++)
        {
            materialRows[index].Visible = false;
        }
    }

    void OnSciencePaymentRequested()
    {
        if (!Component) { return; }
        Component!.Distro.CollectSciencePayment();
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
