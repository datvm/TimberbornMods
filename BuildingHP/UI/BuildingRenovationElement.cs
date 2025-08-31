namespace BuildingHP.UI;

public class BuildingRenovationElementDependencies(
    ILoc t,
    IGoodService goodService,
    DevModeManager devModeManager,
    RenovationPriorityToggleGroupFactory priorityToggleGroupFactory
)
{
    public readonly ILoc T = t;
    public readonly IGoodService GoodService = goodService;
    public readonly DevModeManager DevModeManager = devModeManager;
    public readonly RenovationPriorityToggleGroupFactory PriorityToggleGroupFactory = priorityToggleGroupFactory;
}

public class BuildingRenovationElement : VisualElement, IPrioritizable
{    
    readonly Button btnCancel, btnFinishNow;
    readonly Label lblName, lblProgress, lblMaterials;
    readonly ProgressBar pgbProgress;
    readonly PriorityToggleGroup priorityToggleGroup;
    readonly VisualElement materialPanel;

    readonly BuildingRenovationElementDependencies di;

    public BuildingRenovationComponent? Component { get; private set; }
    public Priority Priority => Component?.Priority ?? Priority.Normal;
    bool priorityControlEnabled;

    public BuildingRenovationElement(BuildingRenovationElementDependencies di)
    {
        this.SetDisplay(false);

        lblName = this.AddGameLabel();

        pgbProgress = this.AddProgressBar().SetMarginBottom();
        lblProgress = pgbProgress.AddProgressLabel();

        materialPanel = this.AddChild().SetMarginBottom();

        var priorityContainer = materialPanel.AddChild().SetMarginBottom(5);
        priorityToggleGroup = di.PriorityToggleGroupFactory.CreateForRenovation(priorityContainer, true);

        lblMaterials = materialPanel.AddGameLabel().SetMarginBottom();

        btnCancel = this.AddStretchedEntityFragmentButton(di.T.T("LV.BHP.CancelReno"), onClick: Cancel, color: EntityFragmentButtonColor.Red);

        btnFinishNow = this.AddGameButton("Finish Now (Dev)", onClick: FinishNow)
            .SetMargin(top: 10)
            .SetDisplay(false);
        this.di = di;
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
        btnFinishNow.SetDisplay(di.DevModeManager.Enabled);

    }

    public void Update()
    {
        if (!Component) { return; }

        var t = di.T;

        var curr = Component.CurrentRenovation;
        if (curr is null)
        {
            this.SetDisplay(false);
            return;
        }

        lblName.text = t.T("LV.BHP.CurrentRenovation", curr.Spec.Title.Value);

        var timer = curr.TimeTrigger;
        if (timer is not null)
        {
            var progress = timer.Progress;
            var remaining = timer.DaysLeft;
            var text = t.T("Time.DaysShort", $"{remaining:0.00}");

            pgbProgress.SetProgress(progress, lblProgress, text);
        }
        else
        {
            pgbProgress.SetProgress(0, lblProgress, t.T("LV.BHP.WaitingForMaterial"));
        }

        if (curr.IsGoodAcquired)
        {
            ToggleMaterialPanel(false);
        }
        else
        {
            lblMaterials.text = GetCostText(curr.Cost);
            ToggleMaterialPanel(true);

            priorityToggleGroup.UpdateGroup();            
        }

        btnCancel.SetDisplay(Component.CanCancel);
        this.SetDisplay(true);
    }

    string GetCostText(IReadOnlyList<GoodAmountSpecNew> cost)
    {
        var goodService = di.GoodService;

        StringBuilder str = new();

        foreach (var c in cost)
        {
            var g = goodService.GetGood(c.Id);

            str.AppendLine($"● {c.Amount} {g.PluralDisplayName.Value}");
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
        
        if (priorityControlEnabled != enabled)
        {
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
    }

    void Cancel()
    {
        if (!Component || !Component.CanCancel) { return; }

        Component.RequestCancelRenovation();
    }

    void FinishNow()
    {
        if (!Component || Component.CurrentRenovation is null) { return; }

        Component.CurrentRenovation.FinishNow();
    }

    public void SetPriority(Priority priority)
    {
        if (!Component || !Component.CanChangePriority) { return; }

        Component.ChangePriority(priority);
    }
}
