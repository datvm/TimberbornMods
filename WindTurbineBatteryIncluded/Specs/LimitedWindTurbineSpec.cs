namespace WindTurbineBatteryIncluded.Specs;

public record LimitedWindTurbineSpec : ComponentSpec
{
    [Serialize]
    public float MaxDegree { get; init; }

    [Serialize]
    public float MinPowerMultiplier { get; init; }

    [Serialize]
    public float MinRequiredWindStrength { get; init; }
}
