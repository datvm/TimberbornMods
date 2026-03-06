namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.Pollination.HiveSpec")]
public record ParsedHiveSpec(
    Int32 PollinationRadius,
    Single HoursBetweenPollinations,
    Single GrowthTimeReduction,
    Int32 PlantsPerPollination
) : ParsedComponentSpec;