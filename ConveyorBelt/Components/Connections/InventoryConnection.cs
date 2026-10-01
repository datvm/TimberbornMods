namespace ConveyorBelt.Components.Connections;

sealed class InventoryConnection(BlockObject block, Inventories inventories, BeltGoodService goods) : IBeltConnection, IBeltSource
{
    readonly InventoryTarget target = new(block, inventories, goods);

    public bool CanTarget(string goodId) => true;

    public bool TryGetTarget(string goodId, out IBeltTarget found)
    {
        found = target;
        return true;
    }

    public void Commit()
    {
    }

    public void CollectDownstream(List<BeltCarrier> into)
    {
    }

    public bool TryTake(out string goodId) => goods.TryTake(block, inventories, out goodId);
}
