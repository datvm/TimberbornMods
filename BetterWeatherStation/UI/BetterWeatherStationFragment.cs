namespace BetterWeatherStation.UI;

[BindFragment]
public class BetterWeatherStationFragment(
    ILoc t,
    WeatherStationInfoService service
) : BaseEntityPanelFragment<BetterWeatherStationComponent>
{

    readonly Dictionary<string, Toggle> chkWeathers = [];

    protected override void InitializePanel()
    {
        panel.AddGameLabel(t.T("Building.WeatherStation.Header")).SetMarginBottom(10);

        AddWeathers("LV.BWS.BenignWeathers", service.BenignWeathers);
        AddWeathers("LV.BWS.HazardousWeathers", service.HazardousWeathers);

        void AddWeathers(string titleKey, IReadOnlyList<WeatherDefinition> weathers)
        {
            var grp = panel.AddChild();
            grp.AddGameLabel((t.T(titleKey) + ":").Bold()).SetMarginBottom(5);

            foreach (var w in weathers)
            {
                var chk = grp.AddToggle(w.Name, onValueChanged: v => OnWeatherChanged(w.Id, v))
                    .SetWidthPercent(100);

                chkWeathers.Add(w.Id, chk);
            }
        }
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is null) { return; }

        foreach (var id in component.WeatherIds)
        {
            if (chkWeathers.TryGetValue(id, out var chk))
            {
                chk.SetValueWithoutNotify(true);
            }
        }
    }

    public override void ClearFragment()
    {
        base.ClearFragment();

        foreach (var chk in chkWeathers.Values)
        {
            chk.SetValueWithoutNotify(false);
        }
    }

    void OnWeatherChanged(string id, bool v)
    {
        if (!component) { return; }
        component!.ToggleWeather(id, v);
    }

}
