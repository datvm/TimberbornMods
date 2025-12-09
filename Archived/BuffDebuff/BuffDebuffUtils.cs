using System.Collections.Frozen;

namespace BuffDebuff;

public static class BuffDebuffUtils
{

    public static PropertyKey<string> IdKey = new("Id");

    public static readonly FrozenDictionary<string, Type> AllTypes = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(s => s.GetTypes())
        .ToLookup(t => t.FullName, t => t)
        .ToFrozenDictionary(q => q.Key, q => q.First());

    public static Type? GetTypeFrom(string typeFullName) => AllTypes.TryGetValue(typeFullName, out var type) ? type : null;

}
