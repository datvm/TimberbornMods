namespace TimberPipes.Models;

public readonly record struct PipeFlowEdge(int A, int B, bool AllowAToB, bool AllowBToA);

public static class PipeFlowSolver
{
    public static float PipeHead(int z, float volume, float extraLift = 0f)
        => z + volume + Math.Max(0f, extraLift);

    public static float RemainingLift(int z, int outletZ, float ratedLift)
        => Math.Max(0f, ratedLift - (z - outletZ));

    public static bool TankConflictsWithPipe(string? tankStoredGoodId, bool tankTakesPipeGood, string? pipeGoodId)
    {
        if (pipeGoodId is null)
        {
            return false;
        }

        if (tankStoredGoodId is not null)
        {
            return tankStoredGoodId != pipeGoodId;
        }

        return !tankTakesPipeGood;
    }

    public static float TankHead(int zBase, float volumeM3, float capacityM3, int heightTiles)
    {
        if (capacityM3 <= 0 || heightTiles <= 0)
        {
            return zBase;
        }

        return zBase + Math.Clamp(volumeM3 / capacityM3, 0f, 1f) * heightTiles;
    }

    public static float PumpHead(int outletZ, float volumeM3)
        => outletZ + Math.Clamp(volumeM3 / PipeFluids.PipeCapacity, 0f, 1f);

    public static float GoodsToVolume(int goods) => goods * PipeFluids.PacketVolume;

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
}
