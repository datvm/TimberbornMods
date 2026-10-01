namespace ConveyorBelt.Components;

sealed class MergerProvider(BeltMerger merger, BeltLinks links) : IBeltConnectionProvider
{
    public int Priority => BeltConnectionPriority.Conveyor;

    public bool TryProvide(BeltApproach approach, out IBeltConnection connection)
    {
        connection = MissingConnection.Instance;
        if (!merger.AcceptsInput(approach.From))
        {
            return false;
        }

        merger.RememberInput(approach.From);
        var output = merger.OutputCells.Length == 0
            ? MissingConnection.Instance
            : links.Resolve(new(merger.Coordinates, merger.OutputCells[0])) ?? MissingConnection.Instance;
        connection = new MergerConnection(merger, output);
        return true;
    }
}

sealed class MergerConnection(BeltMerger merger, IBeltConnection output) : IBeltConnection
{
    public bool CanTarget(string goodId) => merger.Running && output.CanTarget(goodId);

    public bool TryGetTarget(string goodId, out IBeltTarget target)
    {
        target = null!;
        if (!merger.Running)
        {
            return false;
        }

        return output.TryGetTarget(goodId, out target);
    }

    public void Commit() => output.Commit();

    public void CollectDownstream(List<BeltCarrier> into) => output.CollectDownstream(into);
}
