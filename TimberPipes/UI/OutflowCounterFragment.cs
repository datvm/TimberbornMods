namespace TimberPipes.UI;

[BindFragment]
public class OutflowCounterFragment(
    ILoc t,
    VisualElementInitializer veInit,
    IDayNightCycle dayNight
) : IEntityPanelFragment, ILoadableSingleton
{
    const int MaxLimit = 20;

    EntityPanelFragmentElement panel = null!;
    Label tickOutflow = null!;
    Label hour = null!;
    GameSlider limit = null!;
    Label lblLimit = null!;
    IOutflowCounter? counter;
    FlowLimitPipe? limiter;
    bool refreshing;
    float secondsPerHour;

    public void Load()
    {
        secondsPerHour = dayNight.DayLengthInSeconds / 24f;
    }

    public VisualElement InitializeFragment()
    {
        panel = new EntityPanelFragmentElement();
        tickOutflow = panel.AddGameLabel().SetMarginBottom(5);
        hour = panel.AddGameLabel().SetMarginBottom(5);

        var limitPanel = panel.AddChild().SetMarginBottom(5);
        lblLimit = limitPanel.AddLabel().SetMarginBottom(5);
        limit = limitPanel.AddSlider(values: new(0, MaxLimit, 0))
            .RegisterChange(OnLimitChanged);

        panel.Initialize(veInit);
        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        counter = entity.GetComponent<IOutflowCounter>();
        limiter = entity.GetComponentOrNull<FlowLimitPipe>();
        if (counter is null)
        {
            ClearFragment();
            return;
        }

        panel.Visible = true;
        lblLimit.SetDisplay(limiter is not null);
        limit.SetDisplay(limiter is not null);
        Refresh();
    }

    public void ClearFragment()
    {
        counter = null;
        limiter = null;
        panel.Visible = false;
    }

    public void UpdateFragment() => Refresh();

    void Refresh()
    {
        if (counter is null)
        {
            return;
        }

        tickOutflow.text = t.T("LV.TPi.OutflowTick", counter.TickVolume);
        hour.text = t.T("LV.TPi.OutflowHour", counter.HourVolume, ToM3PerSecond(counter.HourVolume));
        if (limiter is not { } flowLimit)
        {
            return;
        }

        refreshing = true;
        var value = Mathf.Clamp(flowLimit.LimitM3PerHour, 0, MaxLimit);
        limit.SetValueWithoutNotify(value);
        lblLimit.text = t.T("LV.TPi.HourlyLimit", FormatLimit(value));

        refreshing = false;
    }

    void OnLimitChanged(float value)
    {
        if (refreshing || limiter is not { } flowLimit)
        {
            return;
        }

        flowLimit.LimitM3PerHour = Mathf.Max(0, value);
        flowLimit.GetComponent<BuildingPipe>()?.PortState?.RefreshPortStatus();
    }

    string FormatLimit(float value)
        => value <= 0
            ? t.T("LV.TPi.Unlimited")
            : t.T("LV.TPi.HourlyLimitValue", value, ToM3PerSecond(value));

    float ToM3PerSecond(float m3PerHour)
        => FlowLimitIo.M3PerHourToM3PerSecond(m3PerHour, secondsPerHour);
}
