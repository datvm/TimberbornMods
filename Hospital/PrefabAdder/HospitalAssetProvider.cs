namespace Hospital.PrefabAdder;

public class HospitalAssetProvider : IAssetProvider
{
    public static readonly ImmutableHashSet<string> HospitalPaths = [
        "Buildings/Wellbeing/DoubleBed/DoubleBed",
        "Buildings/Wellbeing/Hospital/Hospital",
    ];


    public bool IsBuiltIn { get; } = false;
    public OrderedAsset PlaceholderAsset { get; } = new(0, new());

    public IEnumerable<OrderedAsset> LoadAll<T>(string path) where T : UnityEngine.Object
    {
        if (typeof(T) != typeof(GameObject)) { yield break; }

        foreach (var p in HospitalPaths)
        {
            if (TryLoad(p, typeof(T), out var item))
            {
                yield return item;
            }
        }
    }

    public void Reset() { }

    public bool TryLoad(string path, Type type, out OrderedAsset orderedAsset)
    {
        var valid = HospitalPaths.Contains(path) && type == typeof(GameObject);

        orderedAsset = valid ? PlaceholderAsset : default;
        return valid;
    }
}
