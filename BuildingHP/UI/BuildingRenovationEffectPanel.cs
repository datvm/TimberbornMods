namespace BuildingHP.UI;

public class BuildingRenovationEffectPanel : CollapsiblePanel
{

    readonly ILoc t;
    readonly IDayNightCycle dayNightCycle;

    readonly List<IActiveRenovationDescriber> describers = [];
    readonly List<IActiveRenovationsDescriber> multidescribers = [];

    readonly Label lblEffects;

    public BuildingRenovationEffectPanel(ILoc t, IDayNightCycle dayNightCycle)
    {
        this.t = t;
        this.dayNightCycle = dayNightCycle;

        SetTitle(t.T("LV.BHP.ActiveEffects"));
        lblEffects = Container.AddGameLabel();
    }

    public void SetComponent(BuildingRenovationComponent? comp)
    {
        describers.Clear();
        multidescribers.Clear();

        if (comp)
        {
            comp.GetComponentsFast(describers);
            comp.GetComponentsFast(multidescribers);
        }

        Update();
    }

    public void Update()
    {
        if (describers.Count == 0)
        {
            lblEffects.text = t.T("LV.BHP.None");
            return;
        }

        StringBuilder str = new();

        var allDesc = describers
            .Select(q => q.Describe(t, dayNightCycle))
            .Where(q => q is not null)
            .Select(q => q!.Value)
            .Concat(multidescribers.SelectMany(q => q.DescribeAll(t, dayNightCycle)));


        foreach (var (title, details, time) in allDesc)
        {
            str.AppendLine(t.T("LV.BHP.ActiveEffect", title, details));

            if (time is not null)
            {
                str.AppendLine(t.T("LV.BHP.ActiveEffectTime", time.Value));
            }
        }

        lblEffects.text = str.Length == 0 ? t.T("LV.BHP.None") : str.ToString();
    }

    public void Unset()
    {
        describers.Clear();
        multidescribers.Clear();
        Update();
    }

}
