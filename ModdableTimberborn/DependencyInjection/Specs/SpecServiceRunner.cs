namespace ModdableTimberborn.DependencyInjection.Specs;

sealed class SpecServiceRunner(
#pragma warning disable CS9113 // Just for DI
    IEnumerable<ISpecServiceFrontRunner> frontRunners, 
#pragma warning restore CS9113 // Parameter is unread.
    IEnumerable<ISpecServiceTailRunner> tailRunners
) : IBlueprintModifierProvider, ILoadableSingleton
{
    readonly ImmutableArray<ISpecServiceTailRunner> tailRunners = [.. tailRunners];

    public void Load() { }

    public void OnSpecLoaded(SpecService instance)
    {
        foreach (var r in tailRunners)
        {
            r.Run(instance);
        }
    }

    public IEnumerable<string> GetModifiers(string blueprintName) => [];

}
