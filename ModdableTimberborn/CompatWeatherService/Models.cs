namespace ModdableTimberborn.CompatWeatherService;

public readonly record struct CompatWeatherType(string Id, string DisplayLoc, bool IsBenign);

public record CompatWeatherCycle(int Cycle, ImmutableArray<CompatWeatherCycleStage> Stages)
{
    public int Length => Stages.Length == 0 ? 1 : Stages.Sum(s => s.Length);
}

public record CompatWeatherCycleStage(int Cycle, int Stage, int StartDay, int Length, string WeatherId, bool IsBenign)
{
    public int LastDay => StartDay + Length - 1;
}

public record CompatWeatherWarning(CompatWeatherWarningStage Stage, float? DaysToHazardous, string? NextWeatherId);

public record CompatNextWeatherCycleStage(int Cycle, int Stage, int StartDay, int? Length, string? WeatherId, bool? IsBenign)
{

    public static implicit operator CompatNextWeatherCycleStage(CompatWeatherCycleStage stage)
        => new(stage.Cycle, stage.Stage, stage.StartDay, stage.Length, stage.WeatherId, stage.IsBenign);

}

public enum CompatWeatherWarningStage
{
    NoWarning,
    ShowedToday,
    Showing,
    Hazardous,
    NoHazardous,
}