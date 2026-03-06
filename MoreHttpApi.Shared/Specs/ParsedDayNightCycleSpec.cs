namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.TimeSystem.DayNightCycleSpec")]
public record ParsedDayNightCycleSpec(
    Int32 HoursPassedOnNewGame,
    Int32 ConfiguredDayLengthInTicks,
    Int32 ConfiguredDaytimeLengthInHours
) : ParsedComponentSpec;