namespace WeatherScientificProjects.Processors;

readonly record struct TodayForecast(IModdedHazardousWeather Weather, float Chance, int Day, Vector2Int Duration, bool StandardChanceIncreased);

public class WeatherUpgradeProcessor(
    ScientificProjectService projects,
    EventBus eb,
    ISingletonLoader loader,
    ModdableWeatherRegistry registry,
    ModdableHazardousWeatherApproachingTimer hazardTimer,
    ModdableWeatherHistoryProvider history,
    ModdableWeatherService service,
    ILoc t,
    WeatherForecastPanel weatherForecastPanel
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new("WeatherUpgradeProcessor");
    static readonly PropertyKey<string> TodayForecastKey = new("TodayForecast2");

    static readonly int DefaultWarningDays = HazardousWeatherApproachingTimer.ApproachingNotificationDays;
    public static int WarningDays = DefaultWarningDays;

    TodayForecast? TodayForecast
    {
        get;
        set
        {
            field = value;
            var text = value is null ? null : GetForecastText(value.Value);
            weatherForecastPanel.SetForecastText(text);
        }
    }

    string GetForecastText(in TodayForecast forecast)
    {
        var d = forecast.Duration;

        return string.Format(t.T(d.x == d.y ? "LV.WSP.Forecast1DayText" : "LV.WSP.ForecastText"),
            forecast.Chance,
            forecast.Weather.Spec.Display,
            forecast.Duration.x,
            forecast.Duration.y);
    }

    public void Load()
    {
        WarningDays = DefaultWarningDays;

        LoadSavedData();

        // Keep saved forecast, don't call this if there is saved data
        if (TodayForecast is null) { SetWeatherForecast(); }

        eb.Register(this);
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(TodayForecastKey))
        {
            var data = s.Get(TodayForecastKey).Split(';');

            var weatherId = data[0];
            if (registry.WeatherByIds.TryGetValue(weatherId, out var weather)
                && weather is IModdedHazardousWeather haz
                && data.Length == 5)
            {
                var duration = new Vector2Int(int.Parse(data[3]), int.Parse(data[4]));
                TodayForecast = new TodayForecast(haz, float.Parse(data[1]), int.Parse(data[2]), duration, true);
            }
        }
    }

    void SetWeatherForecast()
    {
        if (hazardTimer.GetWeatherStage() != GameWeatherStage.Temperate)
        {
            TodayForecast = null;
            return;
        }

        var cycleDay = hazardTimer._gameCycleService.CycleDay;
        var todayStandardChanceIncreased = TodayForecast?.Day == cycleDay && TodayForecast.Value.StandardChanceIncreased;

        var info = WeatherProjectsUtils.WeatherForecastIds
            .Select(projects.GetProject)
            .Where(q => q.Unlocked)
            .ToList();
        if (info.Count == 0) { return; }

        var chance = info.Sum(q => q.Spec.Parameters[0] * (q.Spec.HasSteps ? q.TodayLevel : 1)); // the minimum

        if (TodayForecast is not null)
        {
            var moreChance = info.Sum(q =>
            {
                return todayStandardChanceIncreased && !q.Spec.HasSteps
                    ? 0
                    : q.Spec.Parameters[1] * (q.Spec.HasSteps ? q.TodayLevel : 1);
            });

            chance = Math.Max(chance, TodayForecast.Value.Chance + moreChance); // It's possible that chance is higher after an upgrade
        }

        if (chance == 0)
        {
            // This should not happen
            Debug.LogWarning("WeatherForecast chance is 0, this should not happen");
            return;
        }

        chance = Math.Min(chance, 1f);

        var forecastHazard = GetForecastHazard(chance);
        var duration = GetDurationRange(chance);
        TodayForecast = new(forecastHazard, chance, cycleDay, duration, true);
    }

    IModdedHazardousWeather GetForecastHazard(float correctChance)
    {
        var isCorrect = correctChance >= .95f || UnityEngine.Random.value < correctChance;

        var correctHazard = history.CurrentHazardousWeather;
        if (isCorrect) { return correctHazard; }

        var skip = UnityEngine.Random.Range(0, registry.HazardousWeathers.Count - 1);
        return registry.HazardousWeathers
            .Where(q => q != correctHazard)
            .Skip(skip)
            .First();
    }

    Vector2Int GetDurationRange(float accuracy)
    {
        var correct = hazardTimer._weatherService.HazardousWeatherDuration;
        if (accuracy >= .95f) { return new(correct, correct); }

        var shiftWindow = (int)((7 + correct) * (1 - accuracy) * 2);

        var shift = UnityEngine.Random.Range(-shiftWindow, 0);
        if (shift + correct < 1) { shift = 1 - correct; }

        return new(correct + shift, correct + shift + shiftWindow);
    }

    [OnEvent]
    public void OnProjectsUnlocked(OnScientificProjectUnlockedEvent ev)
    {
        var id = ev.Project.Id;
        if (WeatherProjectsUtils.WeatherForecastIds.Contains(id))
        {
            SetWeatherForecast();
        }
        else if (WeatherProjectsUtils.EmergencyDrillIds.Contains(id))
        {
            ExtendTemperateWeather(ev.Project);
        }
    }

    [OnEvent]
    public void OnProjectsPaid(OnScientificProjectDailyCostChargedEvent _)
    {
        SetWeatherForecast();
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        if (TodayForecast is not null)
        {
            var (weather, chance, day, duration, _) = TodayForecast.Value;

            // Unlikely user will be able to save inbetween the frame so don't bother saving the standard chance increased
            s.Set(TodayForecastKey, string.Format("{0};{1};{2};{3};{4}", weather.WeatherId, chance, day, duration.x, duration.y));
        }
    }

    void ExtendTemperateWeather(ScientificProjectSpec project)
    {
        var days = Mathf.RoundToInt(project.Parameters[0]);
        service.ExtendTemperateWeather(days);
    }

}
