namespace ModdableBindito.Wrappers;

public class PrefabGroupServiceWrapper(
    ISpecService specService,
    IAssetLoader assetLoader,
    IEnumerable<IPrefabGroupProvider> prefabGroupProviders,
    IEnumerable<IPrefabGroupServiceFrontRunner> frontRunners,
    IEnumerable<IPrefabModifier> prefabModifiers
) : PrefabGroupService(specService, assetLoader, prefabGroupProviders), ILoadableSingleton
{

    void ILoadableSingleton.Load()
    {
        Load(); // This call Load of the base class

        var modifiers = prefabModifiers
            .OrderByDescending(x => x.Priority)
            .ToImmutableArray();

        GameObject[] prefabs = [.. AllPrefabs];
        for (int i = 0; i < prefabs.Length; i++)
        {
            var curr = prefabs[i];
            foreach (var modifier in modifiers)
            {
                curr = modifier.ModifyPrefab(curr);
            }
            prefabs[i] = curr;
        }

        foreach (var runner in frontRunners)
        {
            runner.AfterPrefabLoad(this);
        }
    }

}
