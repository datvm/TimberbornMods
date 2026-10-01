namespace ConveyorBelt.Components.Connections;

sealed class MissingConnection : IBeltConnection
{
    public static readonly MissingConnection Instance = new();
    MissingConnection() { }

    public bool CanTarget(string goodId) => false;

    public bool TryGetTarget(string goodId, out IBeltTarget target)
    {
        target = null!;
        return false;
    }

    public void Commit()
    {
    }

    public void CollectDownstream(List<BeltCarrier> into)
    {
    }
}
