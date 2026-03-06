namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.SleepSystem.SleeperSpec")]
public record ParsedSleeperSpec(
    ParsedContinuousEffectSpec[] SleepOutsideEffects,
    Single MaxOffsetInHours
) : ParsedComponentSpec;