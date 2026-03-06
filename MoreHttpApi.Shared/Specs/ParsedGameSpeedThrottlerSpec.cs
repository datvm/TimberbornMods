namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.Characters.GameSpeedThrottlerSpec")]
public record ParsedGameSpeedThrottlerSpec(
    Int32 MinPopulation,
    Int32 MaxPopulation,
    Single MinGameSpeedScale,
    Single MaxGameSpeedScale
) : ParsedComponentSpec;