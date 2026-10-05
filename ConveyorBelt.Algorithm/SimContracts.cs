namespace ConveyorBelt.Algorithm;

public interface ISimTarget
{
    bool CanAccept(string goodId);

    bool TryAccept(string goodId, float leftoverHours, float spentHours);
}

public interface ISimSource
{
    bool TryTake(out string goodId);
}

public interface ISimLink
{
    bool CanTarget(string goodId);

    bool TryGetTarget(string goodId, out ISimTarget target);

    void Commit();

    void CollectDownstream(List<SimBelt> into);
}

interface ISplitHost
{
    bool Running { get; }

    int Cursor { get; set; }

    IReadOnlyList<SmartSplitPort> Ports { get; }
}

public static class SimLinks
{
    public static ISimLink None { get; } = new NoneLink();

    public static ISimLink ToBelt(SimBelt belt) => new BeltLink(belt);
}

sealed class NoneLink : ISimLink
{
    public bool CanTarget(string goodId) => false;

    public bool TryGetTarget(string goodId, out ISimTarget target)
    {
        target = null!;
        return false;
    }

    public void Commit()
    {
    }

    public void CollectDownstream(List<SimBelt> into)
    {
    }
}

sealed class BeltLink(SimBelt belt) : ISimLink
{
    public bool CanTarget(string goodId) => belt.CanAccept(goodId);

    public bool TryGetTarget(string goodId, out ISimTarget target)
    {
        target = belt;
        return belt.CanAccept(goodId);
    }

    public void Commit()
    {
    }

    public void CollectDownstream(List<SimBelt> into)
    {
        if (!into.Contains(belt))
        {
            into.Add(belt);
        }
    }
}

sealed class SplitLink(ISplitHost host, ISimLink[] outputs) : ISimLink
{
    int pendingCursor;
    ISimLink? pending;

    public bool CanTarget(string goodId)
    {
        if (!host.Running)
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

    public bool TryGetTarget(string goodId, out ISimTarget target)
    {
        target = null!;
        if (!host.Running)
        {
            return false;
        }

        var cursor = host.Cursor;
        var index = SmartSplitPicker.Pick(host.Ports, goodId, i => i < outputs.Length && outputs[i].CanTarget(goodId), ref cursor);
        if (index < 0 || !outputs[index].TryGetTarget(goodId, out var next))
        {
            return false;
        }

        pendingCursor = cursor;
        pending = outputs[index];
        target = next;
        return true;
    }

    public void Commit()
    {
        host.Cursor = pendingCursor;
        pending?.Commit();
        pending = null;
    }

    public void CollectDownstream(List<SimBelt> into)
    {
        foreach (var output in outputs)
        {
            output.CollectDownstream(into);
        }
    }
}
