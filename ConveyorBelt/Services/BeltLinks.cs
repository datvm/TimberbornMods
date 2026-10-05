namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltLinks(IBlockService blocks)
{
    readonly HashSet<Vector3Int> trail = [];

    public ISimLink? Resolve(BeltApproach approach, SimBelt? upstream)
    {
        var root = trail.Count == 0;
        if (!trail.Add(approach.To))
        {
            return null;
        }

        try
        {
            return Pick(approach, upstream);
        }
        finally
        {
            if (root)
            {
                trail.Clear();
            }
        }
    }

    ISimLink? Pick(BeltApproach approach, SimBelt? upstream)
    {
        ISimLink? best = null;
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

                if (!provider.TryProvide(approach, upstream, out var connection))
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
