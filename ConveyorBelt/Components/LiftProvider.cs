namespace ConveyorBelt.Components;

sealed class LiftProvider(BeltLift lift, BeltLinks links) : IBeltConnectionProvider
{
    readonly SplitterProvider splitter = new(lift, links);

    public int Priority => BeltConnectionPriority.Conveyor;

    public bool TryProvide(BeltApproach approach, out IBeltConnection connection)
    {
        if (!lift.SendingOut)
        {
            return splitter.TryProvide(approach, out connection);
        }

        connection = MissingConnection.Instance;
        if (!lift.AcceptsInput(approach.From))
        {
            return false;
        }

        lift.RememberInput(approach.From);
        var output = lift.OutputCells.Length == 0
            ? MissingConnection.Instance
            : links.Resolve(new(lift.Coordinates, lift.OutputCells[0])) ?? MissingConnection.Instance;
        connection = new LiftOutConnection(lift, output);
        return true;
    }
}

sealed class LiftOutConnection(BeltLift lift, IBeltConnection output) : IBeltConnection
{
    public bool CanTarget(string goodId) => lift.Running && output.CanTarget(goodId);

    public bool TryGetTarget(string goodId, out IBeltTarget target)
    {
        target = null!;
        if (!lift.Running)
        {
            return false;
        }

        return output.TryGetTarget(goodId, out target);
    }

    public void Commit() => output.Commit();

    public void CollectDownstream(List<BeltCarrier> into) => output.CollectDownstream(into);
}
