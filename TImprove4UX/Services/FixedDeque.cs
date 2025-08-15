namespace TImprove4UX.Services;

public class FixedDeque<T>(int maxSize) : IEnumerable<T>
{

    readonly LinkedList<T> list = [];
    public int MaxSize { get; private set; } = maxSize;

    public void Add(T item)
    {
        list.AddFirst(item);

        if (list.Count > MaxSize)
        {
            list.RemoveLast();
        }
    }

    public T PopStack()
    {
        ThrowIfEmpty();
        var value = list.First.Value;
        list.RemoveFirst();
        return value;
    }

    public T PeekStack()
    {
        ThrowIfEmpty();
        return list.First.Value;
    }

    public void Resize(int maxSize)
    {
        MaxSize = Math.Clamp(maxSize, 0, int.MaxValue);

        while (list.Count > MaxSize)
        {
            list.RemoveLast();
        }
    }

    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

    public int Count => list.Count;
    public bool Empty => list.Count == 0;

    void ThrowIfEmpty()
    {
        if (Empty)
        {
            throw new InvalidOperationException("Deque is empty.");
        }
    }

}
