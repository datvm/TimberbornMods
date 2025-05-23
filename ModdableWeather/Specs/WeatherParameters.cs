namespace ModdableWeather.Specs;

public record WeatherParameters(
    bool Enabled = true,
    int StartCycle = 0,
    int Chance = 100,
    int MinDay = 10,
    int MaxDay = 20,
    int HandicapPerc = 0,
    int HandicapCycles = 0
);
