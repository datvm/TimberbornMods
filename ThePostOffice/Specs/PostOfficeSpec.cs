namespace ThePostOffice.Specs;

public record PostOfficeSpec : ComponentSpec
{
    [Serialize]
    public float SendHours { get; init; } 

    [Serialize]
    public float RetrieveHours { get; init; } 

    [Serialize]
    public int InventoryCapacity { get; init; } 

    [Serialize]
    public int GlobalCapacityContribution { get; init; } 
}
