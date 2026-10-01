namespace ConveyorBelt.Models;

public readonly record struct BeltApproach(Vector3Int From, Vector3Int To);

public static class BeltConnectionPriority
{
    public const int Conveyor = 10;
    public const int Building = 0;
}

public interface IBeltConnection
{
    bool CanTarget(string goodId);

    bool TryGetTarget(string goodId, out IBeltTarget target);

    void Commit();

    void CollectDownstream(List<BeltCarrier> into);
}

public interface IBeltTarget
{
    bool CanAccept(string goodId);

    bool TryAccept(string goodId, float leftoverHours);
}

public interface IBeltSource
{
    bool TryTake(out string goodId);
}

public interface IBeltConnectionProvider
{
    int Priority { get; }

    bool TryProvide(BeltApproach approach, out IBeltConnection connection);
}

public static class BeltConnectionPick
{
    public static int? Highest(IReadOnlyList<(int Priority, int Id, bool Offered)> offers)
    {
        var best = int.MinValue;
        int? id = null;
        foreach (var offer in offers)
        {
            if (!offer.Offered || offer.Priority < best)
            {
                continue;
            }

            if (offer.Priority == best && id is not null)
            {
                continue;
            }

            best = offer.Priority;
            id = offer.Id;
        }

        return id;
    }
}
