namespace ConveyorBelt.Models;

public readonly record struct BeltApproach(Vector3Int From, Vector3Int To);

public interface IBeltConnectionProvider
{
    int Priority { get; }

    bool TryProvide(BeltApproach approach, SimBelt? upstream, out ISimLink link);
}
