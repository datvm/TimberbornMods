namespace ModdableWeathers.UI.Settings;

public class WeatherSettingsDialog : DialogBoxElement
{
    readonly PanelStack panelStack;
    readonly IContainer container;
    readonly ImmutableArray<IFilterablePanel> panels = [];
    readonly WeatherSettingsDialogFilter filter = new();

    public WeatherSettingsDialog(
        VisualElementInitializer veInit,
        PanelStack panelStack,
        ILoc t,
        ModdableWeatherRegistry weatherRegistry,
        ModdableWeatherModifierRegistry weatherModifierRegistry,
        WeatherHistoryRegistry weatherHistoryRegistry,
        RainSettings rainSettings,
        IContainer container)
    {
        this.panelStack = panelStack;
        this.container = container;
        List<IFilterablePanel> panels = [];

        SetTitle(t.T("LV.MW.ShowSettings"));
        AddCloseButton();
        SetDialogPercentSize(null, .8f);

        var parent = Content;
        parent.AddLabel(t.T("LV.MW.SettingsNote", weatherHistoryRegistry.CycleCount)).SetMarginBottom();

        var exportPanel = container.GetInstance<WeatherSettingsExportPanel>().SetMarginBottom();
        parent.Add(exportPanel);
        exportPanel.ReloadRequested += Reload;

        parent.AddChild(container.GetInstance<WeatherCycleStagesPanel>);
        parent.AddChild(() => new RainSettingsPanel(t, rainSettings));

        AddFilterPanel(parent, t);

        var weathersPanel = parent.AddChild().SetMarginBottom();
        weathersPanel.AddLabelHeader(t.T("LV.MW.Weathers")).SetMarginBottom(10);

        foreach (var w in weatherRegistry.Weathers)
        {
            var el = container.GetInstance<WeatherSettingsPanel>();
            el.Init(w);
            weathersPanel.Add(el);
            panels.Add(el);
        }

        var modifiersPanel = parent.AddChild().SetMarginBottom();
        modifiersPanel.AddLabelHeader(t.T("LV.MW.WeatherModifiers")).SetMarginBottom(10);

        foreach (var m in weatherModifierRegistry.Modifiers)
        {
            var el = container.GetInstance<WeatherModifierSettingsPanel>();
            el.Init(m);
            modifiersPanel.Add(el);
            panels.Add(el);
        }

        this.Initialize(veInit);
        UpdateFilters();

        this.panels = [.. panels];

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

