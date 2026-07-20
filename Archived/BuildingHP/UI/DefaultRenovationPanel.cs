namespace BuildingHP.UI;

public class DefaultRenovationPanelFactory(
    GoodItemFactory goodItemFactory,
    ILoc t,
    RenovationPriorityToggleGroupFactory priorityToggleGroupFactory
)
{

    public DefaultRenovationPanel Create(IRenovationProvider provider)
        => new(provider, goodItemFactory, t, priorityToggleGroupFactory);

}

public class DefaultRenovationPanel : VisualElement, IPrioritizable
{
    readonly ILoc t;
    readonly Label lblTime;
    readonly PriorityToggleGroup priorityToggle;

    public IRenovationProvider Provider { get; }
    public CostBox CostBox { get; }
    public Priority Priority { get; private set; }

    public VisualElement Others { get; private set; }

    public event Action? OnStartRenovationRequested;

    public DefaultRenovationPanel(
        IRenovationProvider provider,
        GoodItemFactory goodItemFactory,
        ILoc t,
        RenovationPriorityToggleGroupFactory priorityToggleGroupFactory
    )
    {
        Provider = provider;
        this.t = t;
        var spec = provider.RenovationSpec;

        var header = this.AddChild().SetMarginBottom();
        header.AddLabelHeader(spec.Title.Value);
        header.AddGameLabel(spec.Description);
        header.AddGameLabel(spec.Flavor.Value.Italic().Color(TimberbornTextColor.Yellow));

        CostBox = new(goodItemFactory);
        Add(CostBox);
        CostBox
            .SetMarginBottom(5)
            .SetMaterials(spec.Cost);

        var timePanel = this.AddRow().SetMarginBottom().AlignItems();
        lblTime = timePanel.AddGameLabel(name: "Time").SetMarginRight(10);
        SetTime(spec.Days);

        var priorityPanel = this.AddRow().AlignItems().SetMarginBottom();
        priorityPanel.AddGameLabel(t.T("LV.BHP.RenoPriority")).SetMarginRight(10);
        priorityToggle = priorityToggleGroupFactory.CreateForRenovationWithEmptyLabel(priorityPanel);
        priorityPanel.Q(className: "priority-toggle-group").style.flexDirection = FlexDirection.Row;
        priorityPanel.Q(className: "priority-toggle-group__toggles-wrapper").style.flexDirection = FlexDirection.Row;

        Others = this.AddChild().SetMarginBottom();

        if (spec.CannotCancel)
        {
            this.AddGameLabel(t.T("LV.BHP.NoCancel"), "NoCancelWarning").SetMarginBottom();
        }
        
        this.AddMenuButton(t.T("LV.BHP.StartRenovation"),
            onClick: () => OnStartRenovationRequested?.Invoke(),
            size: UiBuilder.GameButtonSize.Large);
        this.AddGameLabel(t.T("LV.BHP.RenovationNote"));
    }

    public void SetTime(float days)
    {
        lblTime.text = t.T("LV.BHP.RenoTime", days.ToString("0.00"));
    }

    public void SetPriority(Priority priority)
    {
        Priority = priority;
        priorityToggle.UpdateGroup();
    }

    public void SetToComponent()
    {
        SetPriority(Priority.Normal);
        priorityToggle.Enable(this);
        priorityToggle.UpdateGroup();
    }

    public void CleanUp()
    {
        priorityToggle.Disable();
    }

}