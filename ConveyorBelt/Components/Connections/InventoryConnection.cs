namespace ConveyorBelt.Components.Connections;

sealed class InventoryConnection(BlockObject block, Inventories inventories, BeltGoodService goods) : ISimLink, ISimSource, ISimTarget
{
    public bool CanTarget(string goodId) => true;

    public bool TryGetTarget(string goodId, out ISimTarget found)
    {
        found = this;
        return true;
    }

    public bool CanAccept(string goodId) => true;

    public bool TryAccept(string goodId, float leftoverHours, float spentHours) => goods.TryGive(block, inventories, goodId);

    public void Commit()
    {
    }

    public void CollectDownstream(List<SimBelt> into)
    {
    }

    public bool TryTake(out string goodId) => goods.TryTake(block, inventories, out goodId);
}
