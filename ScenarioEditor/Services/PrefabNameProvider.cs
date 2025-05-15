namespace ScenarioEditor.Services;

public class PrefabNameProvider(
    ISpecService specs,
    IAssetLoader loader
) : ILoadableSingleton
{

    public ImmutableArray<string> Prefabs = [];

    public void Load()
    {
        LoadPrefabs();
    }

    void LoadPrefabs()
    {
        var grps = specs.GetSpecs<PrefabGroupSpec>();
        HashSet<string> prefabs = [];

        foreach (var grp in grps)
        {
            foreach (var path in grp.Paths)
            {
                var obj = loader.Load<GameObject>(path);
                var prefabSpec = obj.GetComponent<PrefabSpec>();

                if (prefabSpec)
                {
                    prefabs.Add(prefabSpec.PrefabName);
                }
            }
        }

        Prefabs = [.. prefabs.OrderBy(q => q, StringComparer.OrdinalIgnoreCase)];
    }

}
