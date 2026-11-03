namespace ThePostOffice.Specs;

public record PostOfficeReceiverSpec : ComponentSpec
{
    [Serialize]
    public float RetrieveHours { get; init; }

    [Serialize]
    public int InventoryCapacity { get; init; }
}
