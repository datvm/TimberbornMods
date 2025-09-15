namespace StreamGaugeTracker.Services;

public readonly record struct StreamGaugeSample(float Depth, float Time, bool IsHazardousWeather);

public interface IReadOnlyLimitedDataQueue : IReadOnlyCollection<StreamGaugeSample>
{
    int MaxSize { get; }
    float Highest { get; }
}

public class LimitedDataQueue(int maxSize) : IReadOnlyLimitedDataQueue
{

    readonly LinkedList<StreamGaugeSample> items = new();
    public int MaxSize { get; private set; } = maxSize;
    public int Count => items.Count;
    public float Highest { get; private set; } = float.MinValue;

    public void Add(StreamGaugeSample item)
    {
        items.AddLast(item);
        if (item.Depth > Highest) { Highest = item.Depth; }
        EnsureSize();
    }

    public void AddRange(IEnumerable<StreamGaugeSample> items)
    {
        foreach (var item in items)
        {
            this.items.AddLast(item);
            if (item.Depth > Highest) { Highest = item.Depth; }
        }
        EnsureSize();
    }

    public IEnumerator<StreamGaugeSample> GetEnumerator() => items.GetEnumerator();

    public void Resize(int newSize)
    {
        MaxSize = newSize;
        EnsureSize();
    }

    void RecalculateHighest() => Highest = items.Any() ? items.Max(q => q.Depth) : float.MinValue;

    void EnsureSize()
    {
        var removedHighest = false;

        while (items.Count > MaxSize)
        {
            if (items.First.Value.Depth == Highest)
            {
                removedHighest = true;
            }

            items.RemoveFirst();
        }

        if (removedHighest)
        {
            RecalculateHighest();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
