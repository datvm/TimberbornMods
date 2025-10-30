namespace ModdableTimberborn.EntityDescribers;

public class EntityEffectDescriberFragment(
    ILoc t,
    IDayNightCycle dayNightCycle
) : IEntityPanelFragment, IEntityFragmentOrder
{
    readonly List<IBaseEntityEffectDescriber> describers = [];
    readonly List<IBaseEntityEffectDescriber> workplaceDescribers = [];

#nullable disable
    EntityPanelFragmentElement panel;
    Label lblEffects;
#nullable enable
    Worker? worker;

    public int Order { get; } = -100;
    public VisualElement Fragment => panel;

    public void ClearFragment()
    {
        describers.Clear();
        workplaceDescribers.Clear();
        worker = null;
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
            entity.GetComponents(describers);
            describers.Sort((a, b) => a.Order.CompareTo(b.Order));

            worker = entity.GetComponent<Worker>();
            if (worker && worker.Workplace)
            {
                worker.Workplace.GetComponents(workplaceDescribers);
                workplaceDescribers.Sort((a, b) => a.Order.CompareTo(b.Order));
            }
            else
            {
                workplaceDescribers.Clear();
            }
        }
        UpdateFragment();
    }

    public void UpdateFragment()
    {
        if (describers.Count == 0 && workplaceDescribers.Count == 0) { return; }

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

        if (workplaceDescribers.Count > 0 && worker)
        {
            foreach (var describer in workplaceDescribers)
            {
                switch (describer)
                {
                    case IWorkplaceWorkerEffectDescriber d:
                        AddDescription(d.DescribeWorkerEffect(worker!, t, dayNightCycle));
                        break;
                    case IWorkplaceEntityMultiEffectsDescriber md:
                        foreach (var desc in md.DescribeAllWorkerEffects(worker!, t, dayNightCycle))
                        {
                            AddDescription(desc);
                        }
                        break;
                }
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
