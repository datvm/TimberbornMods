namespace ModdableWeathers.Helpers;

public static class ModdableWeathersUtils
{

    public static T InstanceOrThrow<T>(this T? instance) where T : class
        => instance ?? throw new InvalidOperationException($"{typeof(T).Name} is not loaded yet!");

    public static T InstanceOrThrow<T>(this T? instance) where T : struct
        => instance ?? throw new InvalidOperationException($"{typeof(T).Name} is not loaded yet!");

    public static void LogVerbose(Func<string> message)
        => TimberUiUtils.LogVerbose(() => $"[{nameof(ModdableWeathers)}]: " + message());

    public static void LogVerbose(Func<string> message, string padding)
        => TimberUiUtils.LogVerbose(() => $"[{nameof(ModdableWeathers)}]: " + PadMessage(message(), padding));

    static string PadMessage(string message, string padding) => Environment.NewLine + message.Replace("\n", "\n" + padding);

    public static float CalculateHandicap(Func<int> getOccurrence, int handicapCycles, Func<int> getInitHandicapPercent)
    {
        if (handicapCycles == 0) { return 1f; }

        var counter = getOccurrence();
        if (counter >= handicapCycles) { return 1f; }

        var initHandicap = getInitHandicapPercent();
        var deltaPerCycle = (100 - initHandicap) / handicapCycles;

        var handicap = initHandicap + (deltaPerCycle * counter);
        return handicap / 100f;
    }

    extension(IModdableWeather weather)
    {
        public bool IsDrought() => weather is GameDroughtWeather;
        public bool IsBadtide() => weather is GameBadtideWeather;
        public bool IsEmpty() => weather is EmptyWeather;

        public bool Match(WeatherSettingsDialogFilter filter) =>
            ((weather.IsHazardous && filter.Hazardous) || (weather.IsBenign && filter.Benign))
            && (filter.Query.Length == 0
                || weather.Spec.Display.Value.Contains(filter.Query, StringComparison.OrdinalIgnoreCase));
    }

    extension(PropertyInfo propertyInfo)
    {
        public bool IsEnabledProperty()
            => propertyInfo.Name == nameof(DefaultModdableWeatherSettings.Enabled)
            && propertyInfo.PropertyType == typeof(bool);
    }
    
    extension<T>(T ve) where T : VisualElement
    {

        public T BorderAndSpace() => ve
            .SetMarginBottom(10)
            .SetPadding(5)
            .SetBorder(color: TextColors.YellowHighlight, width: 1);

    }

}
