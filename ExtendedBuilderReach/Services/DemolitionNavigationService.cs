using System.Buffers;
using System.Diagnostics;

namespace ExtendedBuilderReach.Services;

/// <summary>
/// One registry listener for all extended demolition targets. Local navigation
/// changes visit only targets indexed near the changed map area.
/// </summary>
public class DemolitionNavigationService(INavMeshListenerEntityRegistry navRegistry)
    : INavMeshListener, IUpdatableSingleton, IUnloadableSingleton
{
    const int BucketSize = 8;
    const int DeferredThreshold = 1024;
    const int MaxCandidatesPerFrame = 256;
    const double MaxDrainMilliseconds = 8;

    readonly HashSet<ExtendedDemolishableAccessible> active = [];
    readonly Dictionary<long, HashSet<ExtendedDemolishableAccessible>> buckets = [];
    readonly Dictionary<ExtendedDemolishableAccessible, List<long>> memberships = [];
    readonly HashSet<ExtendedDemolishableAccessible> candidates = [];
    readonly Queue<PendingBatch> pending = [];
    bool registered;

    public void Register(ExtendedDemolishableAccessible listener, BoundingBox bounds)
    {
        if (!active.Add(listener))
        {
            return;
        }

        AddToBuckets(listener, bounds);
        if (!registered)
        {
            navRegistry.RegisterNavMeshListener(this);
            registered = true;
        }
    }

    public void Unregister(ExtendedDemolishableAccessible listener)
    {
        if (!active.Remove(listener))
        {
            return;
        }

        RemoveFromBuckets(listener);
        if (registered && active.Count == 0)
        {
            navRegistry.UnregisterNavMeshListener(this);
            registered = false;
        }
    }

    public void UpdateBounds(ExtendedDemolishableAccessible listener, BoundingBox bounds)
    {
        if (!active.Contains(listener))
        {
            return;
        }

        RemoveFromBuckets(listener);
        AddToBuckets(listener, bounds);
    }

    public void OnNavMeshUpdated(NavMeshUpdate update)
    {
        if (active.Count == 0)
        {
            return;
        }

        candidates.Clear();
        CollectCandidates(update.Bounds);

        // Snapshot before callbacks: UpdateAccesses may modify the buckets.
        var count = candidates.Count;
        if (count == 0)
        {
            return;
        }

        var snapshot = ArrayPool<ExtendedDemolishableAccessible>.Shared.Rent(count);
        candidates.CopyTo(snapshot);
        candidates.Clear();
        if (count >= DeferredThreshold)
        {
            pending.Enqueue(new(update, snapshot, count));
            return;
        }

        try
        {
            for (var i = 0; i < count; i++)
            {
                Dispatch(snapshot[i], update);
            }
        }
        finally
        {
            ArrayPool<ExtendedDemolishableAccessible>.Shared.Return(snapshot, clearArray: true);
        }
    }

    public void UpdateSingleton()
    {
        if (pending.Count == 0)
        {
            return;
        }

        var start = Stopwatch.GetTimestamp();
        var budget = (long)(Stopwatch.Frequency * MaxDrainMilliseconds / 1000);
        var processed = 0;
        while (pending.Count > 0 &&
               processed < MaxCandidatesPerFrame &&
               Stopwatch.GetTimestamp() - start < budget)
        {
            var batch = pending.Peek();
            if (batch.Index >= batch.Count)
            {
                ReturnCompletedBatch();
                continue;
            }

            var listener = batch.Listeners[batch.Index++];
            processed++;
            // A demolition target may be deleted before a deferred callback.
            Dispatch(listener, batch.Update);
        }

        while (pending.Count > 0 && pending.Peek().Index >= pending.Peek().Count)
        {
            ReturnCompletedBatch();
        }
    }

    public void Unload()
    {
        if (registered)
        {
            navRegistry.UnregisterNavMeshListener(this);
            registered = false;
        }

        while (pending.Count > 0)
        {
            ReturnCompletedBatch();
        }

        active.Clear();
        buckets.Clear();
        memberships.Clear();
        candidates.Clear();
    }

    void Dispatch(ExtendedDemolishableAccessible listener, NavMeshUpdate update)
    {
        if (active.Contains(listener))
        {
            // EBR still performs the precise 3D intersection test itself.
            listener.OnNavMeshUpdated(update);
        }
    }

    void ReturnCompletedBatch()
    {
        var batch = pending.Dequeue();
        ArrayPool<ExtendedDemolishableAccessible>.Shared.Return(batch.Listeners, clearArray: true);
    }

    void AddToBuckets(ExtendedDemolishableAccessible listener, BoundingBox bounds)
    {
        List<long> keys = [];
        for (var y = FloorDiv(bounds._minY); y <= FloorDiv(bounds._maxY); y++)
        {
            for (var x = FloorDiv(bounds._minX); x <= FloorDiv(bounds._maxX); x++)
            {
                var key = BucketKey(x, y);
                if (!buckets.TryGetValue(key, out var bucket))
                {
                    bucket = [];
                    buckets.Add(key, bucket);
                }

                bucket.Add(listener);
                keys.Add(key);
            }
        }

        memberships.Add(listener, keys);
    }

    void RemoveFromBuckets(ExtendedDemolishableAccessible listener)
    {
        if (!memberships.Remove(listener, out var keys))
        {
            return;
        }

        foreach (var key in keys)
        {
            if (!buckets.TryGetValue(key, out var bucket))
            {
                continue;
            }

            bucket.Remove(listener);
            if (bucket.Count == 0)
            {
                buckets.Remove(key);
            }
        }
    }

    void CollectCandidates(BoundingBox bounds)
    {
        for (var y = FloorDiv(bounds._minY); y <= FloorDiv(bounds._maxY); y++)
        {
            for (var x = FloorDiv(bounds._minX); x <= FloorDiv(bounds._maxX); x++)
            {
                if (buckets.TryGetValue(BucketKey(x, y), out var bucket))
                {
                    candidates.UnionWith(bucket);
                }
            }
        }
    }

    static int FloorDiv(int coordinate)
    {
        var result = coordinate / BucketSize;
        return coordinate % BucketSize < 0 ? result - 1 : result;
    }

    static long BucketKey(int x, int y) => ((long)x << 32) | (uint)y;

    sealed record PendingBatch(NavMeshUpdate Update, ExtendedDemolishableAccessible[] Listeners, int Count)
    {
        public int Index { get; set; }
    }
}
