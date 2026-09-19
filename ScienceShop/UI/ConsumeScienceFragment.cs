namespace ScienceShop.UI;

[BindFragment]
public class ConsumeScienceFragment(
    ILoc t,
    NamedIconProvider icons,
    VisualElementInitializer veInit
) : BaseEntityPanelFragment<ConsumeScienceManufactory>
{
    IconSpan costIcon = null!;
    Label available = null!;
    Label remaining = null!;
    NineSliceIntegerField cycles = null!;
    Toggle indefinite = null!;
    Button start = null!;
    Button stop = null!;
    bool refreshing;

    protected override void InitializePanel()
    {
        var costRow = panel.AddRow().AlignItems().SetMarginBottom(5);
        costIcon = costRow.AddIconSpan();
        available = costRow.AddGameLabel().SetMargin(left: 8);

        remaining = panel.AddGameLabel().SetMarginBottom(5);

        var batch = panel.AddRow().AlignItems().SetMarginBottom(5);
        batch.AddGameLabel(t.T("LV.ScS.Batch")).SetMarginRight(5);
        cycles = batch.AddIntField(changeCallback: OnCyclesChanged).SetWidth(64).Initialize(veInit);
        indefinite = batch.AddGamePanelToggle(t.T("LV.ScS.Indefinite"), OnIndefiniteChanged).SetMargin(left: 8);

        var buttons = panel.AddRow().AlignItems();
        start = buttons.AddGameButtonPadded(t.T("LV.ScS.Start"), OnStart).SetMarginRight(5);
        stop = buttons.AddGameButtonPadded(t.T("LV.ScS.Stop"), OnStop);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is null || !component.TryGetCurrentSpec(out _))
        {
            ClearFragment();
            return;
        }

        Refresh();
    }

    public override void UpdateFragment()
    {
        if (component is null || !component.TryGetCurrentSpec(out _))
        {
            panel.Visible = false;
            return;
        }

        Refresh();
    }

    void Refresh()
    {
        if (component is null || !component.TryGetCurrentSpec(out var spec))
        {
            panel.Visible = false;
            return;
        }

        panel.Visible = true;
        refreshing = true;
        costIcon.SetScience(icons, spec.ScienceCost.ToString()).SetVertical(false);
        available.text = t.T("LV.ScS.Available", component.Service.AvailableScience);
        remaining.text = component.Indefinite
            ? t.T("LV.ScS.IndefiniteRemaining")
            : t.T("LV.ScS.Remaining", component.Remaining);

        if (cycles.value < 1)
        {
            cycles.SetValueWithoutNotify(1);
        }

        indefinite.SetValueWithoutNotify(component.Indefinite);
        cycles.enabledSelf = !component.Indefinite;
        start.enabledSelf = !component.IsRunning;
        stop.enabledSelf = component.IsRunning;
        refreshing = false;
    }

    void OnCyclesChanged(int value)
    {
        if (refreshing || value < 1)
        {
            return;
        }

        cycles.SetValueWithoutNotify(Math.Max(1, value));
    }

    void OnIndefiniteChanged(bool value)
    {
        if (refreshing || component is null)
        {
            return;
        }

        cycles.enabledSelf = !value;
    }

    void OnStart()
    {
        if (component is null)
        {
            return;
        }

        if (indefinite.value)
        {
            component.StartIndefinite();
        }
        else
        {
            component.Start(Math.Max(1, cycles.value));
        }

        Refresh();
    }

    void OnStop()
    {
        component?.Stop();
        Refresh();
    }
}
