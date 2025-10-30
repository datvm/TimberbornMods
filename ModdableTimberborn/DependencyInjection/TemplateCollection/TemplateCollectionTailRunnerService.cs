namespace ModdableTimberborn.DependencyInjection;

sealed class TemplateCollectionTailRunnerService(IEnumerable<ITemplateCollectionServiceTailRunner> tailRunners) : ITemplateCollectionIdProvider
{
    public IEnumerable<string> GetPrefabGroups() => [];
    readonly ImmutableArray<ITemplateCollectionServiceTailRunner> tailRunners = [.. tailRunners.OrderBy(q => q.Order)];

    public void Run(TemplateCollectionService templateGroupService)
    {
        if (tailRunners.Length == 0) { return; }

        ModdableTimberbornUtils.LogVerbose(() => $"Running {tailRunners.Length} {nameof(ISpecServiceTailRunner)}s");
        foreach (var r in tailRunners)
        {
            r.Run(templateGroupService);
        }
    }

    public IEnumerable<string> GetTemplateCollectionIds() => [];
}