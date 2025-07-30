namespace WeatherEditor.UI;

public class WeatherEditorDialog : DialogBoxElement
{
    private readonly ILoc t;
    private readonly IWeatherModService mod;
    private readonly DialogBoxShower diagShower;
    private readonly IModdableWeatherModService? moddableMod;
    private readonly bool hasModdableWeather;

    Dropdown? cboTemperate, cboNextTemperate;
    Toggle? chkSingleWeather, chkSingleWeatherTemperate;

    ImmutableArray<HazardousWeatherInfo> hazardousWeathers;
    ImmutableArray<WeatherInfo> temperateWeathers;

#nullable disable
    Dropdown cboHazardous;
    IntegerField txtTemperateDuration, txtHazardousDuration;
#nullable enable

    public WeatherEditorDialog(
        ILoc t,
        IWeatherModService mod,
        VisualElementInitializer veInit,
        DropdownItemsSetter dropdownItemsSetter,
        DialogBoxShower diagShower
    )
    {
        this.t = t;
        this.mod = mod;
        this.diagShower = diagShower;
        moddableMod = mod as IModdableWeatherModService;
        hasModdableWeather = moddableMod is not null;

        SetTitle(t.T("LV.WEdit.WeatherEditor"));
        AddCloseButton();

        VisualElement parent = Content;
        AddTemperateWeatherPanel(parent);
        AddHazardousWeatherPanel(parent);
        AddNextCycleWeatherPanel(parent);
        AddSaveButton(parent);

        parent.Initialize(veInit);
        InitializeDropdowns(dropdownItemsSetter);
    }

    void AddSaveButton(VisualElement parent)
    {
        var panel = parent.AddChild();

        var lbl = panel.AddGameLabel(t.T("LV.WEdit.SaveWarning").Color(TimberbornTextColor.Red)).SetMarginBottom(10);
        lbl.style.unityTextAlign = TextAnchor.MiddleCenter;

        panel.AddMenuButton(t.T("LV.WEdit.Edit"), onClick: Edit, stretched: true, size: UiBuilder.GameButtonSize.Large);
    }

    void AddTemperateWeatherPanel(VisualElement parent)
    {
        var panel = parent.AddChild().SetMarginBottom();

        panel.AddGameLabel(t.T("Weather.Temperate").Bold()).SetMarginBottom(10);

        panel.AddGameLabel(t.T("LV.WEdit.CurrentWeather"));
        if (hasModdableWeather)
        {
            cboTemperate = panel.AddDropdown("TemperateWeather");
        }
        else
        {
            panel.AddGameLabel($"{t.T("Weather.Temperate")} ({t.T("LV.WEdit.NoModdableWeather")})");
        }

        panel.AddGameLabel(t.T("LV.WEdit.Duration")).SetMargin(top: 5);
        txtTemperateDuration = panel.AddIntField();
        txtTemperateDuration.SetValueWithoutNotify(mod.TemperateWeatherDuration);
    }

    void AddHazardousWeatherPanel(VisualElement parent)
    {
        var panel = parent.AddChild().SetMarginBottom();
        panel.AddGameLabel(t.T("LV.WEdit.HazardousWeather").Bold()).SetMarginBottom(10);

        panel.AddGameLabel(t.T("LV.WEdit.CurrentWeather"));
        cboHazardous = panel.AddDropdown("HazardousWeather");

        panel.AddGameLabel(t.T("LV.WEdit.Duration")).SetMargin(top: 5);
        txtHazardousDuration = panel.AddIntField();
        txtHazardousDuration.SetValueWithoutNotify(mod.HazardousWeatherDuration);
    }

    void AddNextCycleWeatherPanel(VisualElement parent)
    {
        var panel = parent.AddChild().SetMarginBottom();
        panel.AddGameLabel(t.T("LV.WEdit.NextCycle").Bold()).SetMarginBottom(10);

        if (!hasModdableWeather)
        {
            panel.AddGameLabel(t.T("LV.WEdit.NoModdableWeather"));
            return;
        }

        chkSingleWeather = panel.AddToggle(t.T("LV.MW.SingleWeatherMode"), onValueChanged: _ => SetSingleWeatherUI());
        chkSingleWeatherTemperate = panel.AddToggle(t.T("LV.WEdit.SingleModeTemperate"));
        panel.AddGameLabel(t.T("Weather.Temperate"));
        cboNextTemperate = panel.AddDropdown("NextTemperateWeather");
    }

    void InitializeDropdowns(DropdownItemsSetter dropdownItemsSetter)
    {
        hazardousWeathers = mod.HazardousWeathers;
        temperateWeathers = moddableMod?.TemperateWeathers ?? [];

        cboHazardous.SetItems(dropdownItemsSetter, [.. hazardousWeathers.Select(q => q.DisplayName ?? t.T("LV.WEdit.NoWeather"))]);
        var currHaz = mod.CurrentCycleHazardousWeather.Id;
        for (int i = 0; i < hazardousWeathers.Length; i++)
        {
            if (hazardousWeathers[i].Id == currHaz)
            {
                cboHazardous.SetSelectedItem(i);
                break;
            }
        }

        if (moddableMod is null) { return; }

        IReadOnlyList<string> temperateList = [.. temperateWeathers.Select(q => q.DisplayName ?? t.T("LV.WEdit.NoWeather"))];
        cboTemperate?.SetItems(dropdownItemsSetter, temperateList, moddableMod.CurrentTemperateWeatherName);
        cboNextTemperate?.SetItems(dropdownItemsSetter, temperateList, moddableMod.NextTemperateWeatherName);

        chkSingleWeather?.SetValueWithoutNotify(moddableMod.IsSingleWeather);
        chkSingleWeatherTemperate?.SetValueWithoutNotify(moddableMod.IsSingleWeatherTemperate);
        SetSingleWeatherUI();
    }

    void SetSingleWeatherUI()
    {
        if (chkSingleWeatherTemperate is null) { return; }
        chkSingleWeatherTemperate.enabledSelf = chkSingleWeather?.value == true;
    }

    void Edit()
    {
        try
        {
            SetGameWeather();

            if (hasModdableWeather)
            {
                SetModWeather();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);

            diagShower.Create()
                .SetMessage(t.T(ex.Message))
                .Show();
            return;
        }

        OnUIConfirmed();
    }

    WeatherEditorParameters GetParameters()
    {
        var tempDuration = txtTemperateDuration.value;
        if (tempDuration < 0)
        {
            throw new ArgumentException(t.T("LV.WEdit.DurationNonNeg"));
        }

        var hazDuration = txtHazardousDuration.value;
        if (hazDuration < 0)
        {
            throw new ArgumentException(t.T("LV.WEdit.DurationNonNeg"));
        }

        var hazWeather = hazardousWeathers[cboHazardous.GetSelectedIndex()].HazardousWeather;
        return new(tempDuration, hazWeather, hazDuration);
    }

    void SetGameWeather()
    {
        var ps = GetParameters();
        mod.TemperateWeatherDuration = ps.TemperateDuration;
        mod.HazardousWeatherDuration = ps.HazardousDuration;
        mod.CurrentCycleHazardousWeather = ps.HazardousWeather;
    }

    void SetModWeather()
    {
        moddableMod!.CurrentTemperateWeatherId = temperateWeathers[cboTemperate!.GetSelectedIndex()].Id;
        moddableMod.NextCycle = new(
            chkSingleWeather!.value,
            chkSingleWeatherTemperate!.value,
            temperateWeathers[cboNextTemperate!.GetSelectedIndex()].Id);
    }

}

readonly record struct WeatherEditorParameters(int TemperateDuration, IHazardousWeather HazardousWeather, int HazardousDuration);