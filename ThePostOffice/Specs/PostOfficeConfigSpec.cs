namespace ThePostOffice.Specs;

public record PostOfficeConfigSpec : ComponentSpec
{
    [Serialize]
    public int ParcelWeight { get; init; } = 60;
}
