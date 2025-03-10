global using Timberborn.HazardousWeatherSystem;
global using Timerborn.HazardousWeatherSystemUI;

namespace WeatherScientificProjects.Processors;

readonly record struct TodayForecast(IHazardousWeatherUISpecification Weather, float Chance, int Day, Vector2Int Duration, bool StandardChanceIncreased);

public class WeatherUpgradeProcessor(
    ScientificProjectService projects,
    EventBus eb,
    ISingletonLoader loader,
    HazardousWeatherApproachingTimer hazardTimer,
    DroughtWeatherUISpecification drought,
    BadtideWeatherUISpecification badtide,
    ILoc t,
    WeatherForecastPanel weatherForecastPanel
) : ILoadableSingleton, IUnloadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new("WeatherUpgradeProcessor");
    static readonly PropertyKey<string> TodayForecastKey = new("TodayForecast");

    static readonly int defaultApproachingDay = HazardousWeatherApproachingTimer.ApproachingNotificationDays;
    static readonly FieldInfo approachingDay = typeof(HazardousWeatherApproachingTimer)
        .GetField(nameof(HazardousWeatherApproachingTimer.ApproachingNotificationDays), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

    readonly ImmutableArray<IHazardousWeatherUISpecification> allHazards = [drought, badtide];

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
            t.T(forecast.Weather.NameLocKey),
            forecast.Duration.x,
            forecast.Duration.y);
    }

    public void Load()
    {
        LoadSavedData();

        SetApproachingDay();

        // Keep saved forecast, don't call this if there is saved data
        if (TodayForecast is null) { SetWeatherForecast(); }

        eb.Register(this);
    }

    void LoadSavedData()
    {
        if (!loader.HasSingleton(SaveKey)) { return; }

        var s = loader.GetSingleton(SaveKey);
        if (s.Has(TodayForecastKey))
        {
            var data = s.Get(TodayForecastKey).Split(';');
            var weather = allHazards.FirstOrDefault(q => q.NameLocKey == data[0]);
            if (weather is not null && data.Length == 5) // Don't revert if and return here in case there are more data loading later
            {
                var duration = new Vector2Int(int.Parse(data[3]), int.Parse(data[4]));
                TodayForecast = new TodayForecast(weather, float.Parse(data[1]), int.Parse(data[2]), duration, true);
            }
        }
    }

    void SetWeatherForecast()
    {
        if (!hazardTimer.IsPreWarning(false))
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

    IHazardousWeatherUISpecification GetForecastHazard(float correctChance)
    {
        var isCorrect = correctChance >= .95f || UnityEngine.Random.value < correctChance;

        var hazard = hazardTimer._weatherService._hazardousWeatherService.CurrentCycleHazardousWeather;
        IHazardousWeatherUISpecification correctHazardSpec = hazard is BadtideWeather ? badtide : drought;

        if (isCorrect) { return correctHazardSpec; }

        var skip = UnityEngine.Random.Range(0, allHazards.Length - 1);
        return allHazards
            .Where(q => q != correctHazardSpec)
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

    void SetApproachingDay()
    {
        var info = WeatherProjectsUtils.WeatherWarningExtIds
            .Select(projects.GetProject)
            .Where(q => q.Unlocked);

        int newValue = defaultApproachingDay;
        foreach (var p in info)
        {
            var mul = p.Spec.HasSteps ? p.TodayLevel : 1;
            newValue = (int)MathF.Round(newValue + p.Spec.Parameters[0] * mul);
        }

        approachingDay.SetValue(null, newValue);
    }

    [OnEvent]
    public void OnProjectsUnlocked(OnScientificProjectUnlockedEvent ev)
    {
        var id = ev.Project.Id;
        if (WeatherProjectsUtils.WeatherWarningExtIds.Contains(id))
        {
            SetApproachingDay();
        }
        else if (WeatherProjectsUtils.WeatherForecastIds.Contains(id))
        {
            SetWeatherForecast();
        }
    }

    [OnEvent]
    public void OnProjectsPaid(OnScientificProjectDailyCostChargedEvent _)
    {
        SetApproachingDay();
        SetWeatherForecast();
    }

    public void Unload()
    {
        // Reset to default value so player doesn't exploit it on another save
        approachingDay.SetValue(null, defaultApproachingDay);
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        if (TodayForecast is not null)
        {
            var (weather, chance, day, duration, _) = TodayForecast.Value;

            // Unlikely user will be able to save inbetween the frame so don't bother saving the standard chance increased
            s.Set(TodayForecastKey, string.Format("{0};{1};{2};{3};{4}", weather.NameLocKey, chance, day, duration.x, duration.y));
        }
    }
}
