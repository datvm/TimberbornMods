namespace MoreHttpApi.Shared.BuildingSettings;

public record PopulationCounterSettingsModel(
    HttpPopulationCounterMode CounterMode,
    HttpNumericComparisonMode Mode,
    float Threshold,
    bool CountBeavers,
    bool CountBots,
    bool GlobalMode
) : ComparisonSettingsModel(Mode, Threshold);


public enum HttpPopulationCounterMode
{
    TotalPopulation,
    TotalBeavers,
    Adults,
    Children,
    Bots,
    OccupiedBeds,
    FreeBeds,
    Homeless,
    Jobs,
    Employed,
    Unemployed,
    Vacancies,
    TotalWorkers,
    HealthyWorkers,
    UnhealthyWorkers,
    ContaminatedTotal,
    ContaminatedAdults,
    ContaminatedChildren
}
