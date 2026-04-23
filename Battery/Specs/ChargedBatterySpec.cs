namespace Battery.Specs;

public record ChargedBatterySpec : ComponentSpec
{

    [Serialize]
    public int Charges { get; init; }

    [Serialize]
    public string? ChargedGoodId { get; init; }

    [Serialize]
    public float ChargeEfficiency { get; init; }

    public int RequiredCharges { get; internal set; }

}
