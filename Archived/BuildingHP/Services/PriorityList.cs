namespace BuildingHP.Services;

public interface IReadOnlyPriorityList<T>
{
    IReadOnlyCollection<T> this[int priority] { get; }

    int QueueCount { get; }
    IReadOnlyList<T> PrioritySortedItems { get; }
    IReadOnlyList<T> PriorityDescSortedItems { get; }

    int GetPriority(T item);
    bool TryGetPriority(Func<T, bool> predicate, out int priority, [NotNullWhen(true)] out T? item);
}

public class PriorityList<T> : IReadOnlyPriorityList<T>
{

    readonly LinkedList<T>[] queues;

    public IReadOnlyCollection<T> this[int priority] => queues[priority];
    public int QueueCount { get; }

    public IReadOnlyList<T> PrioritySortedItems => [.. queues.SelectMany(q => q)];
    public IReadOnlyList<T> PriorityDescSortedItems => [.. Enumerable.Reverse(queues).SelectMany(q => q)];

    public PriorityList(int queueCount)
    {
        QueueCount = queueCount;
        queues = new LinkedList<T>[queueCount];
        for (int i = 0; i < queueCount; i++)
        {
            queues[i] = [];
        }
    }

    public int GetPriority(T item)
    {
        for (int i = 0; i < QueueCount; i++)
        {
            if (queues[i].Contains(item))
            {
                return i;
            }
        }

        return -1;
    }

    public bool TryGetPriority(Func<T, bool> predicate, out int priority,[NotNullWhen(true)] out T? item)
    {
        foreach (var q in queues)
        {
            foreach (var i in q)
            {
                if (!predicate(i)) { continue; }

                priority = GetPriority(i);
                item = i!;
                return true;
            }
        }

        priority = -1;
        item = default;
        return false;
    }

    public int Add(T item, int priority)
    {
        var prev = GetPriority(item);
        if (prev > -1 && prev != priority)
        {
            queues[prev].Remove(item);
        }
        else
        {
            queues[priority].AddLast(item);
        }

        return prev;
    }

    public int Remove(T item)
    {
        var priority = GetPriority(item);
        if (priority >= 0)
        {
            queues[priority].Remove(item);
        }

        return priority;
    }

}
