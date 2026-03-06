namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.SoilContaminationSystem.SoilContaminationSimulatorSpec")]
public record ParsedSoilContaminationSimulatorSpec(
    Int32 MaxRangeFromSource,
    Single VerticalSpreadCostMultiplier,
    Single ContaminationSpreadingRate,
    Single ContaminationDecayRate,
    Single ContaminationPositiveEqualizationRate,
    Single ContaminationNegativeEqualizationRate,
    Single MinimumWaterContamination,
    Single ContaminationThreshold
) : ParsedComponentSpec;