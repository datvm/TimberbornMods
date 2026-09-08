namespace BuildingHP.UI;

public class AutoRepairPanel : CollapsiblePanel, IPrioritizable
{
    readonly ILoc t;
    readonly Toggle chkEnabled;
    readonly PriorityToggleGroup grpPriority;
    readonly GameSliderInt autoThreshold;

    BuildingHPRepairComponent? comp;

    public Priority Priority => comp?.AutoRepairPriority ?? Priority.Normal;

    public AutoRepairPanel(
        ILoc t,
        RenovationPriorityToggleGroupFactory priorityFactory
    )
    {
        this.t = t;

        chkEnabled = Container.AddToggle(t.T("LV.BHP.AutoRepair"), onValueChanged: SetAutoRepair)
            .SetMarginBottom(5);

        var priorityContainer = Container.AddChild().SetMarginBottom(5);
        grpPriority = priorityFactory.CreateForRenovation(priorityContainer, true);

        autoThreshold = Container.AddSliderInt(t.T("LV.BHP.AutoRepairHP"), values: new(0, 100, 25))
            .AddEndLabel(v => $"{v}%")
            .RegisterChange(OnThresholdSet);

        SetExpand(false);
    }

    void SetAutoRepair(bool enabled)
    {
        if (!comp) { return; }
        comp.AutoRepair = enabled;
        SetAutoRepairTitle(enabled);
    }

    void OnThresholdSet(int value)
    {
        if (!comp) { return; }
        comp.AutoRepairThreshold = value;
    }

    public void SetComponent(BuildingHPRepairComponent comp)
    {
        if (!comp)
        {
            this.comp = null;
            return;
        }

        this.comp = comp;

        SetAutoRepairTitle(comp.AutoRepair);
        chkEnabled.SetValueWithoutNotify(comp.AutoRepair);
        autoThreshold.SetValueWithoutNotify(comp.AutoRepairThreshold);
        grpPriority.Enable(this);

        Update();
    }

    public void Update()
    {
        if (!comp) { return; }
        grpPriority.UpdateGroup();
    }

    public void Unset()
    {
        comp = null;
        grpPriority.Disable();
    }

    void SetAutoRepairTitle(bool enabled)
    {
        SetTitle(t.T("LV.BHP.AutoRepair") + " " + (enabled ? "✔️" : "✖️"));
    }

    public void SetPriority(Priority priority)
    {
        if (!comp) { return; }
        comp.AutoRepairPriority = priority;
    }
}
