namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.SoilMoistureSystem.SoilMoistureSimulatorSpec")]
public record ParsedSoilMoistureSimulatorSpec(
    Single MinimumWaterContamination,
    Single MaximumWaterContamination,
    Single MoistureDecayRate,
    Single MoistureSpreadingRate,
    Int32 VerticalSpreadCostMultiplier,
    Int32 MaxClusterSaturation,
    Single QuadraticEvaporationCoefficient,
    Single LinearQuadraticCoefficient,
    Single ConstantQuadraticCoefficient,
    Int32 MaxEvaporationSaturation
) : ParsedComponentSpec;