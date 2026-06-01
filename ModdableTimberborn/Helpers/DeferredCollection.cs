namespace ModdableTimberborn.Helpers;

public class DeferredList<T>(List<T> collection) : DeferredCollection<List<T>, T>(collection)
{
    public DeferredList() : this([]) { }
}

public class DeferredHashSet<T>(HashSet<T> collection) : DeferredCollection<HashSet<T>, T>(collection)
{
    public DeferredHashSet() : this([]) { }
}

public class DeferredCollection<TCollection, T>(TCollection collection) : IReadOnlyCollection<T> 
    where TCollection : ICollection<T>
{
    readonly List<PendingOperation> pending = [];

    int enumerationDepth;

    public TCollection Collection => collection;
    public int Count => collection.Count + PendingCountDelta;

    int PendingCountDelta
    {
        get
        {
            if (enumerationDepth == 0)
            {
                return 0;
            }

            var delta = 0;

            foreach (var operation in pending)
            {
                delta += operation.Kind switch
                {
                    PendingOperationKind.Add => 1,
                    PendingOperationKind.Remove => -1,
                    PendingOperationKind.Clear => -collection.Count,
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }

            return delta;
        }
    }

    public void Add(T item)
    {
        if (enumerationDepth > 0)
        {
            pending.Add(PendingOperation.Add(item));
            return;
        }

        collection.Add(item);
    }

    public bool Remove(T item)
    {
        if (enumerationDepth > 0)
        {
            pending.Add(PendingOperation.Remove(item));
            return true;
        }

        return collection.Remove(item);
    }

    public void Clear()
    {
        if (enumerationDepth > 0)
        {
            pending.Add(PendingOperation.Clear());
            return;
        }

        collection.Clear();
    }

    public bool Contains(T item)
    {
        return collection.Contains(item);
    }

    public IEnumerable<T> SafeEnumerate()
    {
        enumerationDepth++;

        try
        {
            foreach (var item in collection)
            {
                yield return item;
            }
        }
        finally
        {
            enumerationDepth--;

            if (enumerationDepth == 0)
            {
                ApplyPendingOperations();
            }
        }
    }

    public IEnumerator<T> GetEnumerator() => SafeEnumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    void ApplyPendingOperations()
    {
        if (pending.Count == 0)
        {
            return;
        }

        foreach (var operation in pending)
        {
            switch (operation.Kind)
            {
                case PendingOperationKind.Add:
                    collection.Add(operation.Item);
                    break;

                case PendingOperationKind.Remove:
                    collection.Remove(operation.Item);
                    break;

                case PendingOperationKind.Clear:
                    collection.Clear();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        pending.Clear();
    }

    readonly record struct PendingOperation(
        PendingOperationKind Kind,
        T Item)
    {
        public static PendingOperation Add(T item) =>
            new(PendingOperationKind.Add, item);

        public static PendingOperation Remove(T item) =>
            new(PendingOperationKind.Remove, item);

        public static PendingOperation Clear() =>
            new(PendingOperationKind.Clear, default!);
    }

    enum PendingOperationKind
    {
        Add,
        Remove,
        Clear,
    }
}