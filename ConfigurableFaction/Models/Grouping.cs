namespace ConfigurableFaction.Models;

public class Grouping<TKey, TValue>(TKey key, IEnumerable<TValue> values) : IGrouping<TKey, TValue>
{
    public TKey Key { get; } = key;
    public IEnumerator<TValue> GetEnumerator() => values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
