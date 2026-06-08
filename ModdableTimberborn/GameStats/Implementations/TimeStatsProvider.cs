namespace ModdableTimberborn.GameStats.Implementations;

public class TimeStatsProvider(
    IDayNightCycle dayNightCycle,
    GameCycleService gameCycleService
) : IFloatGameStatProvider
{
    public IEnumerable<string> AvailableStats => [GameStats.TimePartialDay, GameStats.TimePartialCycleDay, GameStats.TimeTodayHours];

    public float GetStat(string statId) => statId switch
    {
        GameStats.TimePartialDay => dayNightCycle.PartialDayNumber,
        GameStats.TimeTodayHours => dayNightCycle.HoursPassedToday,
        GameStats.TimePartialCycleDay => gameCycleService.PartialCycleDay,
        _ => throw new ArgumentOutOfRangeException(),
    };
}

public class IntTimeStatsProvider(
    IDayNightCycle dayNightCycle,
    GameCycleService gameCycleService
) : IIntGameStatProvider
{
    public IEnumerable<string> AvailableStats => [GameStats.TimeDayNumber, GameStats.TimeCycle, GameStats.TimeCycleDay, GameStats.TimeCycleDuration];

    public int GetStat(string statId) => statId switch
    {
        GameStats.TimeDayNumber => dayNightCycle.DayNumber,
        GameStats.TimeCycle => gameCycleService.Cycle,
        GameStats.TimeCycleDay => gameCycleService.CycleDay,
        GameStats.TimeCycleDuration => gameCycleService._cycleDurationInDays,
        _ => throw new ArgumentOutOfRangeException(),
    };
}

public class PercentTimeStatsProvider(
    IDayNightCycle dayNightCycle,
    GameCycleService gameCycleService
) : IPercentGameStatProvider
{
    public IEnumerable<string> AvailableStats => [GameStats.TimeDayProgress, GameStats.TimeCycleProgress];
    public float GetStat(string statId) => statId switch
    {
        GameStats.TimeDayProgress => dayNightCycle.DayProgress,
        GameStats.TimeCycleProgress => (gameCycleService.PartialCycleDay - 1).PercentOf(gameCycleService._cycleDurationInDays),
        _ => throw new ArgumentOutOfRangeException(),
    };
}
