namespace TimberLive.Helpers;

public static class CollectionExtensions
{

    extension<TKey, TValue>(IDictionary<TKey, TValue> dict)
    {

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (!dict.TryGetValue(key, out var value))
            {
                value = valueFactory(key);
                dict[key] = value;
            }

            return value;
        }

    }

}
