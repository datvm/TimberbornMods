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
        connection = new MergerConnection(merger, output, merger.EntryDirection(approach.From));
        return true;
    }
}

sealed class MergerConnection(BeltMerger merger, IBeltConnection output, Direction3D from) : IBeltConnection
{
    public bool CanTarget(string goodId) => merger.Running && output.CanTarget(goodId);

    public bool TryGetTarget(string goodId, out IBeltTarget target)
    {
        target = null!;
        if (!merger.Running || !output.TryGetTarget(goodId, out var next))
        {
            return false;
        }

        target = new MergerHopTarget(merger, from, next);
        return true;
    }

    public void Commit() => output.Commit();

    public void CollectDownstream(List<BeltCarrier> into) => output.CollectDownstream(into);
}

sealed class MergerHopTarget(BeltMerger merger, Direction3D from, IBeltTarget next) : IBeltTarget
{
    public bool CanAccept(string goodId) => next.CanAccept(goodId);

    public bool TryAccept(string goodId, float leftoverHours, float spentHours)
    {
        var cross = merger.HopHours;
        if (cross > leftoverHours)
        {
            cross = leftoverHours;
        }

        var left = leftoverHours - cross;
        if (!next.TryAccept(goodId, left, spentHours + cross))
        {
            return false;
        }

        merger.RememberHop(goodId, from, spentHours, cross);
        return true;
    }
}
