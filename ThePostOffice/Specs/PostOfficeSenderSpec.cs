namespace ThePostOffice.Specs;

public record PostOfficeSenderSpec : ComponentSpec
{
    [Serialize]
    public float SendHours { get; init; }
}
