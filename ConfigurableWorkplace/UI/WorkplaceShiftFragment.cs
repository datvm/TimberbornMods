namespace ConfigurableWorkplace.UI;

public class WorkplaceShiftFragment(
    ILoc t,
    VisualElementInitializer veInit
) : IEntityPanelFragment
{
    public const int HoursPerDay = 24;
    WorkplaceShiftComponent? comp;

#nullable disable
    EntityPanelFragmentElement panel;
    Toggle chkEnabled;
    TimePickerElement shiftPicker, lunchStartPicker, lunchEndPicker;
#nullable enable

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Background = EntityPanelFragmentBackground.Green,
            Visible = false,
        };

        chkEnabled = panel.AddToggle(text: "LV.CWk.EnableShift".T(t), onValueChanged: OnShiftEnabledChanged)
            .SetMarginBottom(10);

        var hourFormat = "Time.HoursShort".T(t);

        shiftPicker = panel.AddChild<TimePickerElement>()
            .SetMarginBottom(10)
            .SetTexts("LV.CWk.ShiftHours".T(t), hourFormat)
            .RegisterOnValueChanged(OnShiftHoursChanged);

        lunchStartPicker = panel.AddChild<TimePickerElement>()
            .SetMarginBottom(10)
            .SetTexts("LV.CWk.LunchBreakStart".T(t), hourFormat)
            .RegisterOnValueChanged(OnLunchStartHoursChanged);

        lunchEndPicker = panel.AddChild<TimePickerElement>()
            .SetMarginBottom(10)
            .SetTexts("LV.CWk.LunchBreakEnd".T(t), hourFormat)
            .RegisterOnValueChanged(OnLunchEndHoursChanged);

        return panel.Initialize(veInit);
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<WorkplaceShiftComponent>();
        if (!comp) { return; }

        UpdateContent();
        panel.Visible = true;
    }

    public void UpdateFragment() { }

    public void ClearFragment()
    {
        comp = null;
        panel.Visible = false;
    }

    void UpdateContent()
    {
        chkEnabled.value = comp!.EnableCustomShift;

        shiftPicker.SetValueWithoutNotifying(comp.CustomShiftHour);
        lunchStartPicker.SetValuesWithoutNotifying(comp.LunchBreakStart, new(0, comp.LunchBreakEnd));
        lunchEndPicker.SetValuesWithoutNotifying(comp.LunchBreakEnd, new(comp.LunchBreakStart, HoursPerDay));

        SetUiEnabled();
    }

    void OnShiftEnabledChanged(bool enabled)
    {
        if (comp == null) { return; }

        comp.SetEnabledCustomShift(enabled);
        UpdateContent();
    }

    void OnShiftHoursChanged(int hours)
    {
        if (comp == null) { return; }
        comp.CustomShiftHour = hours;
    }

    void OnLunchStartHoursChanged(int hours)
    {
        if (comp == null) { return; }

        comp.LunchBreakStart = hours;
        lunchEndPicker.SetMinMax(new(hours, HoursPerDay));
    }

    void OnLunchEndHoursChanged(int hours)
    {
        if (comp == null) { return; }

        comp.LunchBreakEnd = hours;
        lunchStartPicker.SetMinMax(new(0, hours));
    }

    void SetUiEnabled()
    {
        var enabled = chkEnabled.value;

        shiftPicker.enabledSelf = enabled;
        lunchStartPicker.enabledSelf = enabled;
        lunchEndPicker.enabledSelf = enabled;
    }

}
