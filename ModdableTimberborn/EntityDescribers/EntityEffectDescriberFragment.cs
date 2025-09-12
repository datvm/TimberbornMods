namespace ModdableTimberborn.EntityDescribers;

public class EntityEffectDescriberFragment(
    ILoc t,
    IDayNightCycle dayNightCycle
) : IEntityPanelFragment
{
    readonly List<IBaseEntityEffectDescriber> describers = [];

#nullable disable
    EntityPanelFragmentElement panel;
    Label lblEffects;
#nullable enable

    public void ClearFragment()
    {
        describers.Clear();
        panel.Visible = false;
    }

    public VisualElement InitializeFragment()
    {
        panel = new();

        panel.AddGameLabel(t.T("LV.MT.ActiveEffects")).SetMarginBottom(5);
        lblEffects = panel.AddGameLabel();

        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        if (entity)
        {
            entity.GetComponentsFast(describers);
        }
        UpdateFragment();
    }

    public void UpdateFragment()
    {
        if (describers.Count == 0) { return; }

        StringBuilder str = new();

        foreach (var describer in describers)
        {
            switch (describer)
            {
                case IEntityEffectDescriber d:
                    AddDescription(d.Describe(t, dayNightCycle));
                    break;
                case IEntityMultiEffectsDescriber md:
                    foreach (var desc in md.DescribeAll(t, dayNightCycle))
                    {
                        AddDescription(desc);
                    }
                    break;
            }
        }

        if (str.Length > 0)
        {
            lblEffects.text = str.ToString();
            panel.Visible = true;
        }
        else
        {
            panel.Visible = false;
        }

        void AddDescription(EntityEffectDescription? desc)
        {
            if (desc is null) { return; }
            var (title, details, time) = desc.Value;

            str.AppendLine(t.T("LV.MT.ActiveEffect", title, details));

            if (time.HasValue)
            {
                str.AppendLine(t.T("LV.MT.ActiveEffectTime", time.Value));
            }
        }
    }
}
