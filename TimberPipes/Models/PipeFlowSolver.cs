namespace TimberPipes.Models;

public readonly record struct PipeFlowEdge(
    int A,
    int B,
    bool AllowAToB,
    bool AllowBToA);

public delegate void PipeHeadFill(Span<float> heads, ReadOnlySpan<float> volumes);

public static class PipeFlowSolver
{
    public static float PipeHead(int z, float volume) => z + volume;

    public static bool IsFull(float volume, float capacity)
        => volume >= capacity - PipeFluids.FullEpsilon;

    public static bool TankConflictsWithPipe(string? tankStoredGoodId, bool tankTakesPipeGood, string? pipeGoodId)
        => InventoryConflictsWithPipe(tankStoredGoodId, tankTakesPipeGood, pipeGoodId, emptyMustAccept: true);

    public static bool WellConflictsWithPipe(string? wellStoredGoodId, string? pipeGoodId)
        => InventoryConflictsWithPipe(wellStoredGoodId, takesPipeGood: true, pipeGoodId, emptyMustAccept: false);

    public static bool InventoryConflictsWithPipe(
        string? storedGoodId,
        bool takesPipeGood,
        string? pipeGoodId,
        bool emptyMustAccept)
    {
        if (pipeGoodId is null)
        {
            return false;
        }

        if (storedGoodId is not null)
        {
            return storedGoodId != pipeGoodId;
        }

        return emptyMustAccept && !takesPipeGood;
    }

    public static float TankHead(int zBase, float volumeM3, float capacityM3, int heightTiles)
    {
        if (capacityM3 <= 0 || heightTiles <= 0)
        {
            return zBase;
        }

        return zBase + Math.Clamp(volumeM3 / capacityM3, 0f, 1f) * heightTiles;
    }

    public static float Surface(int z, float volume, float capacity)
        => z + (capacity <= 0 ? 0f : Math.Clamp(volume / capacity, 0f, 1f));

    public static int SliceCount(int heightTiles) => Math.Max(1, heightTiles);

    public static int SliceIndex(int worldZ, int zBase, int heightTiles)
        => Math.Clamp(worldZ - zBase, 0, SliceCount(heightTiles) - 1);

    public static float SliceCapacity(float capacityM3, int heightTiles)
        => capacityM3 / SliceCount(heightTiles);

    public static void PackSlices(float volumeM3, Span<float> slices, float sliceCap)
    {
        var left = Math.Max(0f, volumeM3);
        var cap = Math.Max(0f, sliceCap);
        for (var i = 0; i < slices.Length; i++)
        {
            var take = Math.Min(left, cap);
            slices[i] = take;
            left -= take;
        }
    }

    public static float UnpackSlices(ReadOnlySpan<float> slices)
    {
        var sum = 0f;
        for (var i = 0; i < slices.Length; i++)
        {
            sum += slices[i];
        }

        return sum;
    }

    public static float PumpHead(int outletZ, float volumeM3)
        => outletZ + Math.Clamp(volumeM3 / PipeFluids.PipeCapacity, 0f, 1f);

    public static float GoodsToVolume(int goods) => goods * PipeFluids.PacketVolume;

    public static float FlowCap(int goodsPerTick, float workFactor)
        => Math.Max(0, goodsPerTick) * PipeFluids.PacketVolume * Math.Max(0f, workFactor);

    public static int QuantizePending(ref float pending)
    {
        var packet = PipeFluids.PacketVolume;
        var delta = 0;

        while (pending >= packet)
        {
            pending -= packet;
            delta++;
        }

        while (pending <= -packet)
        {
            pending += packet;
            delta--;
        }

        return delta;
    }

    public static void Run(
        Span<float> volumes,
        ReadOnlySpan<float> capacities,
        ReadOnlySpan<int> z,
        ReadOnlySpan<PipeFlowEdge> edges,
        int pipeCount,
        int flowCount,
        ReadOnlySpan<float> sourceLift,
        ReadOnlySpan<float> qMax,
        int gravitySubsteps,
        float kDt,
        Span<float> remainingLift,
        PipeHeadFill fillHeads)
    {
        var n = volumes.Length;
        var flow = Math.Clamp(flowCount, 0, n);
        var pipes = Math.Clamp(pipeCount, 0, flow);
        var heads = new float[n];
        var steps = Math.Max(1, gravitySubsteps);
        for (var step = 0; step < steps; step++)
        {
            fillHeads(heads, volumes);
            Equalize(volumes, heads, capacities, edges, kDt);
        }

        EqualizeVessels(volumes, capacities, z, edges, flow);
        PushPumps(volumes, capacities, z, edges, sourceLift, qMax, flow);
        ComputeRemainingLift(z, volumes, capacities, edges, sourceLift, remainingLift, pipes);
    }

    public static void Equalize(
        Span<float> volumes,
        ReadOnlySpan<float> heads,
        ReadOnlySpan<float> capacities,
        ReadOnlySpan<PipeFlowEdge> edges,
        float kDt)
    {
        var n = volumes.Length;
        var edgeCount = edges.Length;
        if (n == 0 || edgeCount == 0 || kDt <= 0)
        {
            return;
        }

        var desired = new float[edgeCount];
        var outgoing = new float[n];
        var incoming = new float[n];

        for (var i = 0; i < edgeCount; i++)
        {
            var e = edges[i];
            var q = kDt * (heads[e.A] - heads[e.B]);
            if (q > 0 && !e.AllowAToB)
            {
                q = 0;
            }
            else if (q < 0 && !e.AllowBToA)
            {
                q = 0;
            }

            desired[i] = q;
            if (q > 0)
            {
                outgoing[e.A] += q;
                incoming[e.B] += q;
            }
            else if (q < 0)
            {
                var mag = -q;
                outgoing[e.B] += mag;
                incoming[e.A] += mag;
            }
        }

        var scaleOut = new float[n];
        var scaleIn = new float[n];
        for (var i = 0; i < n; i++)
        {
            scaleOut[i] = outgoing[i] > volumes[i] && outgoing[i] > 0
                ? volumes[i] / outgoing[i]
                : 1f;
            var free = Math.Max(0f, capacities[i] - volumes[i]);
            scaleIn[i] = incoming[i] > free && incoming[i] > 0
                ? free / incoming[i]
                : 1f;
        }

        for (var i = 0; i < edgeCount; i++)
        {
            var e = edges[i];
            var q = desired[i];
            if (q > 0)
            {
                q *= Math.Min(scaleOut[e.A], scaleIn[e.B]);
                volumes[e.A] -= q;
                volumes[e.B] += q;
            }
            else if (q < 0)
            {
                q = -q * Math.Min(scaleOut[e.B], scaleIn[e.A]);
                volumes[e.B] -= q;
                volumes[e.A] += q;
            }
        }

        for (var i = 0; i < n; i++)
        {
            if (volumes[i] < 0)
            {
                volumes[i] = 0;
            }
            else if (volumes[i] > capacities[i])
            {
                volumes[i] = capacities[i];
            }
        }
    }

    public     static void EqualizeVessels(
        Span<float> volumes,
        ReadOnlySpan<float> capacities,
        ReadOnlySpan<int> z,
        ReadOnlySpan<PipeFlowEdge> edges,
        int flowCount)
    {
        var n = volumes.Length;
        if (flowCount < 1 || n == 0 || edges.Length == 0)
        {
            return;
        }

        var adj = Outflows(n, edges);
        var coreVisited = new bool[flowCount];
        List<int> tops = [];
        HashSet<int> topSet = [];

        for (var seed = 0; seed < flowCount; seed++)
        {
            if (coreVisited[seed] || !IsFull(volumes[seed], capacities[seed]))
            {
                continue;
            }

            Queue<int> q = new();
            q.Enqueue(seed);
            coreVisited[seed] = true;
            while (q.Count > 0)
            {
                var i = q.Dequeue();
                var higherFull = false;
                foreach (var to in adj[i])
                {
                    if (to >= flowCount)
                    {
                        continue;
                    }

                    if (IsFull(volumes[to], capacities[to]))
                    {
                        if (z[to] > z[i])
                        {
                            higherFull = true;
                        }

                        if (!coreVisited[to])
                        {
                            coreVisited[to] = true;
                            q.Enqueue(to);
                        }

                        continue;
                    }

                    if (topSet.Add(to))
                    {
                        tops.Add(to);
                    }
                }

                if (!higherFull && topSet.Add(i))
                {
                    tops.Add(i);
                }
            }
        }

        if (tops.Count == 0)
        {
            return;
        }

        var height = new float[tops.Count];
        for (var i = 0; i < tops.Count; i++)
        {
            var top = tops[i];
            height[i] = Surface(z[top], volumes[top], capacities[top]);
        }

        var order = new int[tops.Count];
        for (var i = 0; i < order.Length; i++)
        {
            order[i] = i;
        }

        Array.Sort(order, (a, b) =>
        {
            var cmp = height[b].CompareTo(height[a]);
            return cmp != 0 ? cmp : tops[a].CompareTo(tops[b]);
        });

        foreach (var i in order)
        {
            TryMoveTop(tops[i], volumes, capacities, z, adj, flowCount);
        }
    }

    public static void PushPumps(
        Span<float> volumes,
        ReadOnlySpan<float> capacities,
        ReadOnlySpan<int> z,
        ReadOnlySpan<PipeFlowEdge> edges,
        ReadOnlySpan<float> sourceLift,
        ReadOnlySpan<float> qMax,
        int flowCount)
    {
        var n = volumes.Length;
        if (flowCount < 1 || n == 0 || edges.Length == 0)
        {
            return;
        }

        var adj = Outflows(n, edges);
        for (var pump = 0; pump < n; pump++)
        {
            var lift = pump < sourceLift.Length ? sourceLift[pump] : 0f;
            var cap = pump < qMax.Length ? qMax[pump] : 0f;
            if (lift <= 0 || cap <= 0 || volumes[pump] <= PipeFluids.MoveEpsilon)
            {
                continue;
            }

            var dest = FindPumpFrontier(pump, lift, volumes, capacities, z, adj, flowCount);
            if (dest < 0)
            {
                continue;
            }

            var delta = Math.Min(volumes[pump], Math.Min(cap, capacities[dest] - volumes[dest]));
            if (delta <= PipeFluids.MoveEpsilon)
            {
                continue;
            }

            volumes[pump] -= delta;
            volumes[dest] += delta;
        }
    }

    public static void ComputeRemainingLift(
        ReadOnlySpan<int> z,
        ReadOnlySpan<float> volumes,
        ReadOnlySpan<float> capacities,
        ReadOnlySpan<PipeFlowEdge> edges,
        ReadOnlySpan<float> sourceLift,
        Span<float> extra,
        int pipeCount)
    {
        var n = z.Length;
        extra.Clear();
        if (n == 0 || extra.Length < n || sourceLift.Length < n)
        {
            return;
        }

        var adj = Outflows(n, edges);
        Queue<int> q = new();
        var queued = new bool[n];
        for (var i = 0; i < n; i++)
        {
            if (sourceLift[i] <= 0)
            {
                continue;
            }

            extra[i] = sourceLift[i];
            q.Enqueue(i);
            queued[i] = true;
        }

        while (q.Count > 0)
        {
            var from = q.Dequeue();
            queued[from] = false;
            var canTransmit = sourceLift[from] > 0 || (from < pipeCount && IsFull(volumes[from], capacities[from]));
            if (!canTransmit)
            {
                continue;
            }

            foreach (var to in adj[from])
            {
                if (to >= pipeCount)
                {
                    continue;
                }

                var transmitted = extra[from] - Math.Max(0, z[to] - z[from]);
                if (transmitted <= extra[to] + 1e-5f)
                {
                    continue;
                }

                extra[to] = transmitted;
                if (!queued[to])
                {
                    q.Enqueue(to);
                    queued[to] = true;
                }
            }
        }
    }

    static void TryMoveTop(
        int src,
        Span<float> volumes,
        ReadOnlySpan<float> capacities,
        ReadOnlySpan<int> z,
        List<int>[] adj,
        int flowCount)
    {
        if (volumes[src] <= PipeFluids.MoveEpsilon)
        {
            return;
        }

        var srcH = Surface(z[src], volumes[src], capacities[src]);
        var seen = new bool[flowCount];
        Queue<int> q = new();
        if (src < flowCount && IsFull(volumes[src], capacities[src]))
        {
            seen[src] = true;
            q.Enqueue(src);
        }

        foreach (var to in adj[src])
        {
            if (to >= flowCount || !IsFull(volumes[to], capacities[to]) || seen[to])
            {
                continue;
            }

            seen[to] = true;
            q.Enqueue(to);
        }

        var dest = -1;
        var destH = 0f;
        while (q.Count > 0)
        {
            var i = q.Dequeue();
            foreach (var to in adj[i])
            {
                if (to >= flowCount || to == src)
                {
                    continue;
                }

                if (IsFull(volumes[to], capacities[to]))
                {
                    if (!seen[to])
                    {
                        seen[to] = true;
                        q.Enqueue(to);
                    }

                    continue;
                }

                var h = Surface(z[to], volumes[to], capacities[to]);
                if (h >= srcH - PipeFluids.MoveEpsilon)
                {
                    continue;
                }

                if (dest < 0 || h < destH - PipeFluids.MoveEpsilon)
                {
                    dest = to;
                    destH = h;
                }
            }
        }

        if (dest < 0)
        {
            return;
        }

        srcH = Surface(z[src], volumes[src], capacities[src]);
        destH = Surface(z[dest], volumes[dest], capacities[dest]);
        if (srcH <= destH + PipeFluids.MoveEpsilon)
        {
            return;
        }

        var room = capacities[dest] - volumes[dest];
        var delta = Math.Min(volumes[src], Math.Min(room, 0.5f * (srcH - destH)));
        if (delta <= PipeFluids.MoveEpsilon)
        {
            return;
        }

        volumes[src] -= delta;
        volumes[dest] += delta;
    }

    static int FindPumpFrontier(
        int pump,
        float lift,
        ReadOnlySpan<float> volumes,
        ReadOnlySpan<float> capacities,
        ReadOnlySpan<int> z,
        List<int>[] adj,
        int flowCount)
    {
        var n = volumes.Length;
        var bestP = new float[n];
        var queued = new bool[n];
        Queue<int> q = new();
        bestP[pump] = lift;
        q.Enqueue(pump);
        queued[pump] = true;

        var dest = -1;
        var destH = 0f;
        var destZ = 0;
        var destFill = 0f;

        while (q.Count > 0)
        {
            var i = q.Dequeue();
            queued[i] = false;
            var p = bestP[i];
            var transmits = i == pump || (i < flowCount && IsFull(volumes[i], capacities[i]));
            if (!transmits)
            {
                continue;
            }

            foreach (var to in adj[i])
            {
                if (to >= flowCount || to == pump)
                {
                    continue;
                }

                var nextP = p - Math.Max(0, z[to] - z[i]);
                if (nextP < 0)
                {
                    continue;
                }

                if (!IsFull(volumes[to], capacities[to]))
                {
                    ConsiderFrontier(
                        to,
                        z[to],
                        Fill01(volumes[to], capacities[to]),
                        Surface(z[to], volumes[to], capacities[to]),
                        ref dest,
                        ref destH,
                        ref destZ,
                        ref destFill);
                    continue;
                }

                if (nextP <= bestP[to] + 1e-5f)
                {
                    continue;
                }

                bestP[to] = nextP;
                if (!queued[to])
                {
                    q.Enqueue(to);
                    queued[to] = true;
                }
            }
        }

        return dest;
    }

    static float Fill01(float volume, float capacity)
        => capacity <= 0 ? 0f : Math.Clamp(volume / capacity, 0f, 1f);

    static void ConsiderFrontier(
        int to,
        int toZ,
        float fill,
        float h,
        ref int dest,
        ref float destH,
        ref int destZ,
        ref float destFill)
    {
        if (dest < 0)
        {
            dest = to;
            destH = h;
            destZ = toZ;
            destFill = fill;
            return;
        }

        if (h < destH - PipeFluids.MoveEpsilon)
        {
            dest = to;
            destH = h;
            destZ = toZ;
            destFill = fill;
            return;
        }

        if (h > destH + PipeFluids.MoveEpsilon)
        {
            return;
        }

        if (toZ != destZ)
        {
            return;
        }

        if (fill < destFill - PipeFluids.MoveEpsilon || (Math.Abs(fill - destFill) <= PipeFluids.MoveEpsilon && to < dest))
        {
            dest = to;
            destH = h;
            destZ = toZ;
            destFill = fill;
        }
    }

    static List<int>[] Outflows(int n, ReadOnlySpan<PipeFlowEdge> edges)
    {
        var adj = new List<int>[n];
        for (var i = 0; i < n; i++)
        {
            adj[i] = [];
        }

        for (var e = 0; e < edges.Length; e++)
        {
            var edge = edges[e];
            if (edge.AllowAToB && edge.A >= 0 && edge.A < n && edge.B >= 0 && edge.B < n)
            {
                adj[edge.A].Add(edge.B);
            }

            if (edge.AllowBToA && edge.B >= 0 && edge.B < n && edge.A >= 0 && edge.A < n)
            {
                adj[edge.B].Add(edge.A);
            }
        }

        return adj;
    }
}
