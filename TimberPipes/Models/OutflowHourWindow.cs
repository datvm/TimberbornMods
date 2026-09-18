namespace TimberPipes.Models;

public interface IOutflowCounter
{
    float HourVolume { get; }
    float TickVolume { get; }
    void AddOutflow(float volumeM3);
    void SetTickOutflow(float volumeM3);
    void Prune(float nowPartialDay);
}

public sealed class OutflowHourWindow
{
    public const float HourInDays = 1f / 24f;

    readonly List<(float Time, float Volume)> samples = [];
    float total;

    public float HourVolume => total;

    public void Add(float nowPartialDay, float volumeM3)
    {
        Prune(nowPartialDay);
        if (volumeM3 <= PipeFluids.MoveEpsilon)
        {
            return;
        }

        samples.Add((nowPartialDay, volumeM3));
        total += volumeM3;
    }

    public void Prune(float nowPartialDay)
    {
        var cutoff = nowPartialDay - HourInDays;
        var i = 0;
        while (i < samples.Count && samples[i].Time < cutoff)
        {
            total -= samples[i].Volume;
            i++;
        }

        if (i > 0)
        {
            samples.RemoveRange(0, i);
        }

        if (total < 0f)
        {
            total = 0f;
        }
    }
}

public static class FlowLimitIo
{
    public static bool IsClosed(float limitM3PerHour, float hourVolume)
        => limitM3PerHour > 0f && hourVolume >= limitM3PerHour - PipeFluids.MoveEpsilon;

    public static float Remaining(float limitM3PerHour, float hourVolume)
    {
        if (limitM3PerHour <= 0f)
        {
            return float.PositiveInfinity;
        }

        return Math.Max(0f, limitM3PerHour - hourVolume);
    }

    public static float M3PerHourToM3PerSecond(float m3PerHour, float secondsPerHour)
    {
        if (secondsPerHour <= 0f)
        {
            return 0f;
        }

        return m3PerHour / secondsPerHour;
    }

    public static float TickVolumeToM3PerSecond(float tickVolumeM3, float tickIntervalSeconds)
    {
        if (tickIntervalSeconds <= 0f)
        {
            return 0f;
        }

        return tickVolumeM3 / tickIntervalSeconds;
    }

    public static float CapCounted(float q, int from, Span<float> remaining, Span<float> counted)
    {
        if (q <= PipeFluids.MoveEpsilon || from < 0)
        {
            return q <= 0f ? 0f : q;
        }

        if (from < remaining.Length)
        {
            var cap = remaining[from];
            if (!float.IsPositiveInfinity(cap))
            {
                q = Math.Min(q, cap);
                if (q <= PipeFluids.MoveEpsilon)
                {
                    return 0f;
                }

                remaining[from] -= q;
            }
        }

        if (from < counted.Length)
        {
            counted[from] += q;
        }

        return q;
    }

    public static float CountPath(float q, List<int> nodes, Span<float> remaining, Span<float> counted)
    {
        if (q <= PipeFluids.MoveEpsilon)
        {
            return 0f;
        }

        foreach (var node in nodes)
        {
            if (node < 0 || node >= remaining.Length)
            {
                continue;
            }

            var cap = remaining[node];
            if (!float.IsPositiveInfinity(cap))
            {
                q = Math.Min(q, cap);
            }
        }

        if (q <= PipeFluids.MoveEpsilon)
        {
            return 0f;
        }

        foreach (var node in nodes)
        {
            CapCounted(q, node, remaining, counted);
        }

        return q;
    }
}
