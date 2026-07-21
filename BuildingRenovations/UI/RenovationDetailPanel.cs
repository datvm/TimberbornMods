namespace BuildingRenovations.UI;

[BindTransient]
public class RenovationDetailPanel(
    ILoc t,
    PriorityToggleGroupFactory priorityToggleGroupFactory,
    BuilderPrioritySpriteLoader builderPrioritySpriteLoader,
    IGoodService goodService
) : VisualElement, IPrioritizable
{
#nullable disable
    Label lblTitle, lblDescription, lblExtra, lblFlavor, lblTime, lblUnavailable;
    VisualElement costBox, actionsPanel;
    PriorityToggleGroup priorityToggle;
#nullable enable

    BuildingRenovationComponent? building;
    RenovationBase? renovation;
    Action? onStarted;

    public Priority Priority { get; private set; } = Priority.Normal;

    public RenovationDetailPanel Init()
    {
        lblTitle = this.AddLabelHeader("").SetMarginBottom(20);

        lblDescription = this.AddGameLabel().SetMarginBottom();
        lblExtra = this.AddGameLabel().SetMarginBottom().SetDisplay(false);

        lblFlavor = this.AddGameLabel().SetMarginBottom();

        costBox = this.AddRow().AlignItems().SetMarginBottom();

        var timePanel = this.AddRow().SetMarginBottom().AlignItems();
        lblTime = timePanel.AddGameLabel(name: "Time").SetMarginRight(10);

        lblUnavailable = this.AddGameLabel().SetMarginBottom().SetDisplay(false);

        actionsPanel = this.AddChild();

        var priorityPanel = actionsPanel.AddRow().AlignItems().SetMarginBottom();
        priorityPanel.AddGameLabel(t.T("LV.BRe.RenoPriority")).SetMarginRight(10);
        priorityToggle = priorityToggleGroupFactory.CreatePriorityToggle(
            priorityPanel,
            builderPrioritySpriteLoader,
            "Empty");
        priorityPanel.Q(className: "priority-toggle-group").style.flexDirection = FlexDirection.Row;
        priorityPanel.Q(className: "priority-toggle-group__toggles-wrapper").style.flexDirection = FlexDirection.Row;

        actionsPanel.AddMenuButton(t.T("LV.BRe.StartRenovation"),
            onClick: Start,
            size: UiBuilder.GameButtonSize.Large);
        actionsPanel.AddGameLabel(t.T("LV.BRe.RenovationNote"));

        return this;
    }

    public void Set(
        BuildingRenovationComponent building,
        RenovationBase renovation,
        string? unavailableReason = null,
        Action? onStarted = null)
    {
        this.building = building;
        this.renovation = renovation;
        this.onStarted = onStarted;

        var spec = renovation.Spec;
        lblTitle.text = spec.Title.Value;
        lblDescription.text = spec.Description;

        var extra = renovation.GetExtraDescription(building);
        if (string.IsNullOrWhiteSpace(extra))
        {
            lblExtra.SetDisplay(false);
        }
        else
        {
            lblExtra.text = extra;
            lblExtra.SetDisplay(true);
        }

        if (string.IsNullOrEmpty(spec.FlavorLoc))
        {
            lblFlavor.SetDisplay(false);
        }
        else
        {
            lblFlavor.text = spec.Flavor.Value.Italic().Color(TimberbornTextColor.Yellow);
            lblFlavor.SetDisplay(true);
        }

        SetMaterials(spec.Cost);
        lblTime.text = t.T("LV.BRe.RenoTime", spec.Days.ToString("0.00"));

        if (unavailableReason is null)
        {
            lblUnavailable.SetDisplay(false);
            actionsPanel.SetDisplay(true);

            Priority = Priority.Normal;
            priorityToggle.Enable(this);
            priorityToggle.UpdateGroup();
        }
        else
        {
            lblUnavailable.text = unavailableReason.Color(TimberbornTextColor.Red);
            lblUnavailable.SetDisplay(true);
            actionsPanel.SetDisplay(false);
            priorityToggle.Disable();
        }
    }

    void SetMaterials(IEnumerable<GoodAmountSpec> goods)
    {
        costBox.Clear();

        foreach (var g in goods)
        {
            costBox.AddIconSpan().SetMarginRight()
                .SetGood(goodService, g.Id, g.Amount.ToString(), showName: true);
        }
    }

    public void Unset()
    {
        building = null;
        renovation = null;
        onStarted = null;
        priorityToggle.Disable();
    }

    public void SetPriority(Priority priority)
    {
        Priority = priority;
        priorityToggle.UpdateGroup();
    }

    void Start()
    {
        if (building is null || renovation is null) { return; }

        // StartRenovation re-validates CanRenovate + GetUnavailableReason.
        building.StartRenovation(renovation.Id, Priority);
        onStarted?.Invoke();
    }
}
