namespace ModdableWeather.Services;

public class ModdableWeatherDevModule(
    ModdableWeatherHistoryProvider history,
    DialogBoxShower diag
) : IDevModule
{

    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Moddable Weather: Show cycle info", ShowCurrentWeather))
            .AddMethod(DevMethod.Create("Moddable Weather: Print log", PrintWeatherLog))
            .Build();
    }

    void ShowCurrentWeather()
    {
        var currCycleDetails = history.CurrentCycleDetails;
        var currCycle = currCycleDetails.Cycle;

        var text = $"""
            Cycle: {currCycle.Cycle}
            - Temperate Weather: {currCycleDetails.TemperateWeather}, {currCycle.TemperateWeatherDuration} days
            - Hazardous Weather: {currCycleDetails.HazardousWeather}, {currCycle.HazardousWeatherDuration} days
            - Next Temperate Weather: {history.NextCycleWeather}
            """;

        diag.Create()
            .SetMessage(text)
            .Show();
    }

    void PrintWeatherLog()
    {
        StringBuilder str = new("""
            ===
            Moddable History Log
            ===

            """);

        foreach (var cycle in history.Cycles)
        {
            str.AppendLine($"""
Cycle {cycle.Cycle}:
- Temperate Weather: {cycle.TemperateWeather}
- Hazardous Weather: {cycle.HazardousWeather}
===
""");
        }

        str.AppendLine($"Next Temperate Weather: {history.NextCycleWeather}");
        str.Append("===");

        Debug.Log(str);
    }

}
