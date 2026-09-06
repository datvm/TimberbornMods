namespace ConveyorBelt.Components;

public interface IIgnoredBeltInventory
{
    IEnumerable<Inventory> GetIgnoredInventories();
}
