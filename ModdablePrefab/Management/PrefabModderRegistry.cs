global using System.Collections.Frozen;

namespace ModdablePrefab;

public class PrefabModderRegistry(IEnumerable<IPrefabModder> prefabModders) : ILoadableSingleton
{

    public static FrozenDictionary<Type, ImmutableArray<IPrefabModder>>? ModdersByTypes { get; private set; }

    public void Load()
    {
        Dictionary<Type, List<IPrefabModder>> modders = [];

        foreach (var m in prefabModders)
        {
            foreach (var t in m.PrefabTypes)
            {
                if (!modders.TryGetValue(t, out var list))
                {
                    modders[t] = list = [];
                }

                list.Add(m);
            }
        }

        ModdersByTypes = modders.ToFrozenDictionary(
            q => q.Key,
            q => q.Value.ToImmutableArray());
    }

}
