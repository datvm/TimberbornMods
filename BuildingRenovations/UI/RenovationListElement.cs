namespace BuildingRenovations.UI;

[BindTransient]
public class RenovationListElement(
    ILoc t,
    IDayNightCycle dayNightCycle,
    RenovationRegistry renoRegistry
) : CollapsiblePanel
{
#nullable disable
    Label lblActive, lblLog;
#nullable enable

    public BuildingRenovationComponent? Component { get; private set; }

    public RenovationListElement Init()
    {
        SetTitle(t.T("LV.BRe.CompletedReno"));

        Container.AddGameLabel(t.T("LV.BRe.ActiveReno"));
        lblActive = Container.AddGameLabel(t.T("LV.BRe.None"), "ActiveReno").SetMarginBottom(10);

        Container.AddGameLabel(t.T("LV.BRe.RenoLog"));
        lblLog = Container.AddGameLabel(t.T("LV.BRe.None"), "RenoLog");

        SetExpand(false);
        return this;
    }

    public void SetComponent(BuildingRenovationComponent comp)
    {
        Component = comp;
        comp.RenovationFinished += OnChanged;
        comp.RenovationCancelled += OnChanged;
        comp.RenovationRemoved += OnChanged;
        comp.RenovationExpired += OnChanged;
        UpdateContent();
    }

    void OnChanged(string _) => UpdateContent();

    void UpdateContent()
    {
        if (!Component) { return; }

        var hasActive = Component!.ActiveRenovations.Count > 0;
        var completed = Component.Records.NewestFirst.ToList();
        var hasCompleted = completed.Count > 0;

        // Empty history is noise — only show when there is something to list.
        this.SetDisplay(hasActive || hasCompleted);
        if (!hasActive && !hasCompleted) { return; }

        lblActive.text = hasActive
            ? string.Join(Environment.NewLine, Component.ActiveRenovations.Select(r => $"• {GetTitle(r)}"))
            : t.T("LV.BRe.None");

        if (!hasCompleted)
        {
            lblLog.text = t.T("LV.BRe.None");
        }
        else
        {
            var currTime = dayNightCycle.PartialDayNumber;
            lblLog.text = string.Join(Environment.NewLine, completed.Select(r =>
            {
                var passedTime = currTime - r.Time;
                return t.T("LV.BRe.RenoLogEntry", GetTitle(r.Id), passedTime.ToString("0.00"));
            }));
        }
    }

    string GetTitle(string id) => renoRegistry.Renovations[id].Name;

    public void Unset()
    {
        this.SetDisplay(false);

        if (!Component) { return; }

        Component!.RenovationFinished -= OnChanged;
        Component.RenovationCancelled -= OnChanged;
        Component.RenovationRemoved -= OnChanged;
        Component.RenovationExpired -= OnChanged;
        Component = null;
    }
}
