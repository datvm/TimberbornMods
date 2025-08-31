namespace BuildingHP.UI;

public class BuildingRenovationEffectPanel : CollapsiblePanel
{

    readonly ILoc t;
    readonly IDayNightCycle dayNightCycle;

    readonly List<IActiveRenovationDescriber> describers = [];

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

        if (comp)
        {
            comp.GetComponentsFast(describers);
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

        foreach (var item in describers)
        {
            var desc = item.Describe(t, dayNightCycle);
            if (desc is null) { continue; }

            var (title, details, time) = desc.Value;
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
        Update();
    }

}
