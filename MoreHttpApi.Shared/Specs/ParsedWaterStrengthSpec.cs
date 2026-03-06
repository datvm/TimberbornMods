namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.WaterSourceSystem.WaterStrengthSpec")]
public record ParsedWaterStrengthSpec(
    Single MaxWaterSourceStrength,
    Single MaxWaterSourceChangePerSecond,
    Single MinWaterSourceChangeScaler
) : ParsedComponentSpec;