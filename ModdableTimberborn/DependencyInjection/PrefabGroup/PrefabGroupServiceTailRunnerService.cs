namespace ModdableTimberborn.DependencyInjection.PrefabGroup;

sealed class PrefabGroupServiceTailRunnerService(IEnumerable<IPrefabGroupServiceTailRunner> tailRunners) : ILoadableSingleton, IPrefabGroupProvider
{
    public IEnumerable<string> GetPrefabGroups() => [];
    readonly ImmutableArray<IPrefabGroupServiceTailRunner> tailRunners = [.. tailRunners];

    public void Load()
    {
    }

    public void Run(PrefabGroupService prefabGroupService)
    {
        foreach (var r in tailRunners)
        {
            r.Run(prefabGroupService);
        }
    }

}