namespace ConveyorBelt.Components.Connections;

sealed class DirectBeltConnection(BeltCarrier target) : IBeltConnection
{
    public bool CanTarget(string goodId) => target.CanAccept(goodId);

    public bool TryGetTarget(string goodId, out IBeltTarget found)
    {
        found = target;
        return target.CanAccept(goodId);
    }

    public void Commit()
    {
    }

    public void CollectDownstream(List<BeltCarrier> into)
    {
        if (!into.Contains(target))
        {
            into.Add(target);
        }
    }
}
