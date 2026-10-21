namespace ThePostOffice.Specs;

public record PostOfficeCapacitySpec : ComponentSpec
{
    [Serialize]
    public int Contribution { get; init; }
}
