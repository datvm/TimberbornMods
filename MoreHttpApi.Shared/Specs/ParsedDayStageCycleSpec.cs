namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.SkySystem.DayStageCycleSpec")]
public record ParsedDayStageCycleSpec(
    Single SunriseSunsetLengthInHours,
    Single TransitionLengthInHours
) : ParsedComponentSpec;