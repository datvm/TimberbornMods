namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.WaterSystem.WaterSimulatorSpec")]
public record ParsedWaterSimulatorSpec(
    Single WaterFlowFactor,
    Single WaterSpillThreshold,
    Single NormalEvaporationSpeed,
    Single FastEvaporationDepthThreshold,
    Single FastEvaporationSpeed,
    Single OutflowBalancingScaler,
    Single SoftDamOffset,
    Single HardDamOffset,
    Single MaxHardDamDecrease,
    Single FlowChangeLimit,
    Single OverflowPressureFactor,
    Single DiffusionOutflowLimit,
    Single DiffusionDepthLimit,
    Single DiffusionRate,
    Single MaxWaterContamination
) : ParsedComponentSpec;