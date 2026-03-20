namespace ModdableWeathers.UI.Settings;

public class WeatherSettingsDialog(
    VisualElementInitializer veInit,
    PanelStack panelStack,
    ILoc t,
    ModdableWeatherRegistry weatherRegistry,
    ModdableWeatherModifierRegistry weatherModifierRegistry,
    WeatherHistoryRegistry weatherHistoryRegistry,
    RainSettings rainSettings,
    IContainer container
) : DialogBoxElement
{
    ImmutableArray<IFilterablePanel> panels = [];
    readonly WeatherSettingsDialogFilter filter = new();

    public void Init()
    {
        List<IFilterablePanel> panels = [];

        SetTitle(t.T("LV.MW.ShowSettings"));
        AddCloseButton();
        SetDialogPercentSize(null, .8f);

        var parent = Content;
        parent.AddLabel(t.T("LV.MW.SettingsNote", weatherHistoryRegistry.CycleCount)).SetMarginBottom();

        parent.AddChild(container.GetInstance<GlobalSettingsPanel>);

        var exportPanel = container.GetInstance<WeatherSettingsExportPanel>().SetMarginBottom();
        parent.Add(exportPanel);
        exportPanel.ReloadRequested += Reload;

        parent.AddChild(container.GetInstance<GeneralWeatherSettingsPanel>);
        parent.AddChild(container.GetInstance<WeatherCycleStagesPanel>);
        parent.AddChild(() => new RainSettingsPanel(t, rainSettings));

        AddFilterPanel(parent, t);

        var weathersPanel = parent.AddChild().SetMarginBottom();
        weathersPanel.AddLabelHeader(t.T("LV.MW.Weathers")).SetMarginBottom(10);

        foreach (var w in weatherRegistry.Weathers)
        {
            var el = container.GetInstance<WeatherSettingsPanel>();
            el.OnEnabledChanged += OnEnabledChanged;

            el.Init(w);
            weathersPanel.Add(el);
            panels.Add(el);
        }

        var modifiersPanel = parent.AddChild().SetMarginBottom();
        modifiersPanel.AddLabelHeader(t.T("LV.MW.WeatherModifiers")).SetMarginBottom(10);

        foreach (var m in weatherModifierRegistry.Modifiers)
        {
            var el = container.GetInstance<WeatherModifierSettingsPanel>();
            el.OnEnabledChanged += OnEnabledChanged;

            el.Init(m);
            modifiersPanel.Add(el);
            panels.Add(el);
        }

        this.Initialize(veInit);
        UpdateFilters();

        this.panels = [.. panels];
        OnEnabledChanged();
    }

    void OnEnabledChanged()
    {
        foreach (var el in panels)
        {
            switch (el)
            {
                case WeatherSettingsPanel wp:
                    var disabled = !wp.Entity.Enabled;

                    if (disabled)
                    {
                        wp.SetDisabled(true);
                    }
                    else
                    {
                        wp.SetDisabled(false);
                        wp.SetTags([
                            t.T("LV.MW.ModifiersShort"),
                            string.Join(" / ", GetEnabledModifiersFor(wp.Entity.Id))
                        ]);
                    }

                    break;
                case WeatherModifierSettingsPanel mp:
                    var weathers = GetEnabledWeathersForModifier(mp.Entity.Id).ToArray();
                    if (weathers.Length > 0)
                    {
                        mp.SetDisabled(false);
                        mp.SetTags([t.T("LV.MW.WeathersShort"), string.Join(" / ", weathers)]);
                    }
                    else
                    {
                        mp.SetDisabled(true);
                    }

                    break;
            }
        }
    }

    IEnumerable<string> GetEnabledModifiersFor(string weatherId)
    {
        foreach (var mod in weatherModifierRegistry.Modifiers)
        {
            if (mod.Settings.Weathers.TryGetValue(weatherId, out var wInfo))
            {
                if (wInfo.Lock || !wInfo.Enabled) { continue; }

                yield return mod.Spec.Name.Value;
            }
        }
    }

    IEnumerable<string> GetEnabledWeathersForModifier(string modifierId)
    {
        var mod = weatherModifierRegistry.ModifiersById[modifierId];

        var enabledWeathers = mod.Settings.Weathers
            .Where(kv => kv.Value.Enabled)
            .OrderBy(kv => !kv.Value.Lock)
            .Select(kv => kv.Key)
            .ToArray();

        foreach (var wId in enabledWeathers)
        {
            if (weatherRegistry.WeathersById.TryGetValue(wId, out var weather))
            {
                yield return weather.Spec.Display.Value;
            }
        }
    }

    void Reload()
    {
        Close();

        var shower = container.GetInstance<WeatherSettingsDialogShower>();
        shower.ShowDialog();
    }

    void AddFilterPanel(VisualElement parent, ILoc t)
    {
        var panel = parent.AddChild().SetMarginBottom();
        panel.AddLabel(t.T("LV.MW.Filters"));

        var row = panel.AddRow();
        row.AddTextField(name: "FilterName", changeCallback: OnFilterKeywordChanged).SetFlexGrow().SetFlexShrink();
        row.AddToggle(t.T("LV.MW.BenignWeather"), onValueChanged: v => OnFilterWeatherTypeChanged(true, v)).SetValueWithoutNotify(true);
        row.AddToggle(t.T("LV.MW.HazardousWeather"), onValueChanged: v => OnFilterWeatherTypeChanged(false, v)).SetValueWithoutNotify(true);
    }

    void OnFilterKeywordChanged(string v)
    {
        filter.Query = v.Trim();
        UpdateFilters();
    }

    void OnFilterWeatherTypeChanged(bool benign, bool value)
    {
        if (benign)
        {
            filter.Benign = value;
        }
        else
        {
            filter.Hazardous = value;
        }
        UpdateFilters();
    }

    void UpdateFilters()
    {
        foreach (var p in panels)
        {
            p.Filter(filter);
        }
    }

    public async Task ShowAsync() => await ShowAsync(initializer: null, panelStack);

}

