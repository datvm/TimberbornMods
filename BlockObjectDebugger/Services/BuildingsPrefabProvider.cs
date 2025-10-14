
namespace BlockObjectDebugger.Services;

public class BuildingsPrefabProvider : IAssetProvider
{
    public bool IsBuiltIn { get; }

    public IEnumerable<OrderedAsset> LoadAll<T>(string path) where T : UnityEngine.Object
    {
        if (typeof(T) != typeof(GameObject)) { yield break; }

        foreach (var o in ModUtils.OccupationStrings)
        {
            if (TryLoad(ModUtils.GetBuildingPath(o), typeof(GameObject), out var a))
            {
                yield return a;
            }
        }
    }

    public void Reset() { }

    public bool TryLoad(string path, Type type, out OrderedAsset orderedAsset)
    {        
        if (type != typeof(GameObject) ||
            !ModUtils.IsBuildingPath(path, out var occupation))
        {
            orderedAsset = default;
            return false;
        }

        var oEnum = Enum.Parse<BlockOccupations>(occupation);
        var go = new BlockObjectDebuggerPrefab(oEnum).Build();
        orderedAsset = new(0, go);
        return true;
    }



}
