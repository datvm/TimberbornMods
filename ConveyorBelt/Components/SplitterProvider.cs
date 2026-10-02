namespace ConveyorBelt.Components;

interface IBeltSplitter
{
    bool Running { get; }

    int Cursor { get; set; }

    IReadOnlyList<SmartSplitPort> SplitPorts { get; }

    Vector3Int Coordinates { get; }

    Vector3Int[] OutputCells { get; }

    bool AcceptsInput(Vector3Int from);

    IBeltTarget Present(int output, IBeltTarget next);
}

sealed class SplitterProvider(IBeltSplitter splitter, BeltLinks links) : IBeltConnectionProvider
{
    public int Priority => BeltConnectionPriority.Conveyor;

    public bool TryProvide(BeltApproach approach, out IBeltConnection connection)
    {
        connection = MissingConnection.Instance;
        if (!splitter.AcceptsInput(approach.From))
        {
            return false;
        }

        var outputs = new IBeltConnection[splitter.OutputCells.Length];
        for (var i = 0; i < outputs.Length; i++)
        {
            outputs[i] = links.Resolve(new(splitter.Coordinates, splitter.OutputCells[i])) ?? MissingConnection.Instance;
        }

        connection = new SplitterConnection(splitter, outputs);
        return true;
    }
}

sealed class SplitterConnection(IBeltSplitter splitter, IBeltConnection[] outputs) : IBeltConnection
{
    int pendingCursor;
    IBeltConnection? pending;

    public bool CanTarget(string goodId)
    {
        if (!splitter.Running)
        {
            return false;
        }

        for (var i = 0; i < outputs.Length; i++)
        {
            if (outputs[i].CanTarget(goodId))
            {
                return true;
            }
        }

        return false;
    }

    public bool TryGetTarget(string goodId, out IBeltTarget target)
    {
        target = null!;
        if (!splitter.Running)
        {
            return false;
        }

        var cursor = splitter.Cursor;
        var index = SmartSplitPicker.Pick(splitter.SplitPorts, goodId, i => i < outputs.Length && outputs[i].CanTarget(goodId), ref cursor);
        if (index < 0 || !outputs[index].TryGetTarget(goodId, out var next))
        {
            return false;
        }

        pendingCursor = cursor;
        pending = outputs[index];
        target = splitter.Present(index, next);
        return true;
    }

    public void Commit()
    {
        splitter.Cursor = pendingCursor;
        pending?.Commit();
        pending = null;
    }

    public void CollectDownstream(List<BeltCarrier> into)
    {
        foreach (var output in outputs)
        {
            output.CollectDownstream(into);
        }
    }
}

sealed class SplitterHopTarget(BeltSplitter splitter, Direction3D to, IBeltTarget next) : IBeltTarget
{
    public bool CanAccept(string goodId) => next.CanAccept(goodId);

    public bool TryAccept(string goodId, float leftoverHours, float spentHours)
    {
        var cross = splitter.HopHours;
        if (cross > leftoverHours)
        {
            cross = leftoverHours;
        }

        var left = leftoverHours - cross;
        if (!next.TryAccept(goodId, left, spentHours + cross))
        {
            return false;
        }

        splitter.RememberHop(goodId, to, spentHours, cross);
        return true;
    }
}
