namespace TImprove4UX.Services;

public class FixedDeque<T>(int maxSize) : IEnumerable<T>
{

    readonly LinkedList<T> list = [];

    public void Add(T item)
    {
        list.AddFirst(item);

        if (list.Count > maxSize)
        {
            list.RemoveLast();
        }
    }

    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

    public int Count => list.Count;

}
