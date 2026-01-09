namespace Timberborn.Persistence;

public static class TimberUIPersistencesExtensions
{
    public const char DefaultSeparator = '|';

    extension(IObjectLoader loader)
    {   

        public KeyValuePair<TKey, TValue> GetPair<TKey, TValue>(PropertyKey<string> key, char separator = DefaultSeparator)
            where TKey : IConvertible
            where TValue : IConvertible
        {
            var str = loader.Get(key).Split(separator);
            return new KeyValuePair<TKey, TValue>(
                (TKey)Convert.ChangeType(str[0], typeof(TKey)),
                (TValue)Convert.ChangeType(str[1], typeof(TValue))
            );
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> GetPairs<TKey, TValue>(ListKey<string> key, char separator = DefaultSeparator)
            where TKey : IConvertible
            where TValue : IConvertible =>
            loader.Get(key)
                .Select(str =>
                {
                    var split = str.Split(separator);
                    return new KeyValuePair<TKey, TValue>(
                        (TKey)Convert.ChangeType(split[0], typeof(TKey)),
                        (TValue)Convert.ChangeType(split[1], typeof(TValue))
                    );
                });

    }

    extension(IObjectSaver saver)
    {
        public void SetPair<TKey, TValue>(PropertyKey<string> key, KeyValuePair<TKey, TValue> pair, char separator = DefaultSeparator)
            where TKey : IConvertible
            where TValue : IConvertible
            => saver.Set(key, $"{pair.Key}{separator}{pair.Value}");

        public void SetPairs<TKey, TValue>(ListKey<string> key, IEnumerable<KeyValuePair<TKey, TValue>> pairs, char separator = DefaultSeparator)
            where TKey : IConvertible
            where TValue : IConvertible
            => saver.Set(key, [.. pairs.Select(pair => $"{pair.Key}{separator}{pair.Value}")]);

    }

}