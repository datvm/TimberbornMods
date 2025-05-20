namespace ModdableWeather.Specs;

public record WeatherParameters(
    bool Enabled,
    int StartCycle,
    int Chance,
    int MinDay,
    int MaxDay,
    int HandicapPerc,
    int HandicapCycles
);
