namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltLinks(IBlockService blocks)
{
    readonly HashSet<Vector3Int> trail = [];

    public IBeltConnection? Resolve(BeltApproach approach)
    {
        var root = trail.Count == 0;
        if (!trail.Add(approach.To))
        {
            return null;
        }

        try
        {
            return Pick(approach);
        }
        finally
        {
            if (root)
            {
                trail.Clear();
            }
        }
    }

    IBeltConnection? Pick(BeltApproach approach)
    {
        IBeltConnection? best = null;
        var bestPriority = int.MinValue;
        foreach (var obj in blocks.GetObjectsAt(approach.To))
        {
            if (!obj || !obj.IsFinished)
            {
                continue;
            }

            List<IBeltConnectionProvider> providers = [];
            obj.GetComponents(providers);
            foreach (var provider in providers)
            {
                if (provider.Priority < bestPriority)
                {
                    continue;
                }

                if (provider.Priority == bestPriority && best is not null)
                {
                    continue;
                }

                if (!provider.TryProvide(approach, out var connection))
                {
                    continue;
                }

                best = connection;
                bestPriority = provider.Priority;
            }
        }

        return best;
    }
}
