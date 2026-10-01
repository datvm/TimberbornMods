namespace ConveyorBelt.Components.Connections;

sealed class InventoryTarget(BlockObject block, Inventories inventories, BeltGoodService goods) : IBeltTarget
{
    public bool CanAccept(string goodId) => true;

    public bool TryAccept(string goodId, float leftoverHours) => goods.TryGive(block, inventories, goodId);
}
