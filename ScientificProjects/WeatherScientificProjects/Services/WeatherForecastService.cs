namespace WeatherScientificProjects.Services;

public class WeatherForecastService(
    ISingletonLoader loader,
    WeatherForecastPanel weatherForecastPanel,
    ModdableWeatherRegistry registry,
    ModdableWeatherHistoryProvider history,
    HazardousWeatherApproachingTimer hazardTimer
) : DefaultProjectUpgradeListener, ILoadableSingleton, ISaveableSingleton
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
            weatherForecastPanel.SetForecast(value);
        }
    }

    public override FrozenSet<string> UnlockListenerIds { get; } = WeatherProjectsUtils.WeatherForecastUnlockIds;
    public override FrozenSet<string> DailyListenerIds { get; } = [WeatherProjectsUtils.WeatherForecast3Id];

    public override void Load()
    {
        base.Load();

        WarningDays = DefaultWarningDays;
        LoadSavedData();
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

    IModdedHazardousWeather GetForecastHazard(float correctChance)
    {
        var isCorrect = registry.HazardousWeathers.Count <= 1 || correctChance >= .95f || UnityEngine.Random.value < correctChance;

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
        var correct = history.CurrentCycle.HazardousWeatherDuration;
        if (accuracy >= .95f) { return new(correct, correct); }

        var shiftWindow = (int)((7 + correct) * (1 - accuracy) * 2);

        var shift = UnityEngine.Random.Range(-shiftWindow, 0);
        if (shift + correct < 1) { shift = 1 - correct; }

        return new(correct + shift, correct + shift + shiftWindow);
    }

    protected override void ProcessActiveProjects(IReadOnlyList<ScientificProjectInfo> activeProjects, ScientificProjectSpec? newUnlock, ActiveProjectsSource source)
    {
        if (hazardTimer.GetModdableWeatherStage() != GameWeatherStage.Temperate)
        {
            TodayForecast = null;
            return;
        }

        if (activeProjects.Count == 0) { return; }

        var cycleDay = hazardTimer._gameCycleService.CycleDay;
        var todayStandardChanceIncreased = TodayForecast?.Day == cycleDay && TodayForecast.Value.StandardChanceIncreased;

        var chance = activeProjects.Sum(q => q.Spec.Parameters[0] * (q.Spec.HasSteps ? q.Levels.Today : 1)); // the minimum

        if (TodayForecast is not null)
        {
            var moreChance = activeProjects.Sum(q =>
            {
                return todayStandardChanceIncreased && !q.Spec.HasSteps
                    ? 0
                    : q.Spec.Parameters[1] * (q.Spec.HasSteps ? q.Levels.Today : 1);
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
}
public readonly record struct TodayForecast(IModdedHazardousWeather Weather, float Chance, int Day, Vector2Int Duration, bool StandardChanceIncreased);
