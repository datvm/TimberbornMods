namespace BuildingHP.UI;

public class RenovationListElement : CollapsiblePanel
{
    private readonly ILoc t;
    private readonly IDayNightCycle dayNightCycle;
    private readonly RenovationSpecService renovationSpecService;
    readonly Label lblActive, lblLog;

    public BuildingRenovationComponent? Component { get; private set; }

    public RenovationListElement(ILoc t, IDayNightCycle dayNightCycle, RenovationSpecService renovationSpecService)
    {
        this.t = t;
        this.dayNightCycle = dayNightCycle;
        this.renovationSpecService = renovationSpecService;

        SetTitle(t.T("LV.BHP.CompletedReno"));

        Container.AddGameLabel(t.T("LV.BHP.ActiveReno"));
        lblActive = Container.AddGameLabel(t.T("LV.BHP.None"), "ActiveReno").SetMarginBottom(10);

        Container.AddGameLabel(t.T("LV.BHP.RenoLog"));
        lblLog = Container.AddGameLabel(t.T("LV.BHP.None"), "RenoLog");

        SetExpand(false);
    }

    public void SetComponent(BuildingRenovationComponent comp)
    {
        Component = comp;
        comp.RenovationDone += OnRenovationChanged;
        UpdateContent();
    }

    void OnRenovationChanged(BuildingRenovation obj) => UpdateContent();

    void UpdateContent()
    {
        if (!Component) { return; }

        lblActive.text = Component.ActiveRenovations.Count == 0
            ? t.T("LV.BHP.None")
            : string.Join(Environment.NewLine, Component.ActiveRenovations
                .Select(r => $"• {GetTitle(r)}"));

        var completed = Component.AllCompletedRenovations;
        if (completed.Count == 0)
        {
            lblLog.text = t.T("LV.BHP.None");
        }
        else
        {
            var currTime = dayNightCycle.PartialDayNumber;
            lblLog.text = string.Join(Environment.NewLine, completed
                .Select(r =>
                {
                    var passedTime = currTime - r.Time;
                    return t.T("LV.BHP.RenoLogEntry", GetTitle(r.Id), passedTime.ToString("0.00"));
                }));
        }
    }

    string GetTitle(string id) => renovationSpecService.Renovations[id].Title.Value;

    public void Unset()
    {
        if (!Component) { return; }

        Component.RenovationDone -= OnRenovationChanged;
        Component = null;
    }

}
