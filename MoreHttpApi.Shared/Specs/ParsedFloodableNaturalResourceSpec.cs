namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.NaturalResourcesMoisture.FloodableNaturalResourceSpec")]
public record ParsedFloodableNaturalResourceSpec(
    Int32 MinWaterHeight,
    Int32 MaxWaterHeight,
    Single DaysToDie
) : ParsedComponentSpec;