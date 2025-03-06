
namespace ScientificProjects.Buffs;

public abstract class CommonStepProjectBuff<TBuff, TInstance>(
    ISingletonLoader loader,
    IBuffService buffs,
    ScientificProjectService projects,
    EventBus eb
) : CommonProjectBuff<TBuff, TInstance>(loader, buffs, projects, eb)
    where TBuff : CommonStepProjectBuff<TBuff, TInstance>
    where TInstance : BuffInstance<IEnumerable<ScientificProjectInfo>, TBuff>, new()
{
    protected abstract ImmutableHashSet<string> StepProjectIds { get; }

    protected override IEnumerable<ScientificProjectInfo> GetRelevantProjects()
    {
        var baseProjects = base.GetRelevantProjects();

        var stepProjects = StepProjectIds.Select(projects.GetProject)
            .OrderBy(q => q.Spec.Order)
            .Where(q => q.TodayLevel > 0);

        return [.. baseProjects, .. stepProjects];
    }

    [OnEvent]
    public void OnDailyProjectChanged(OnScientificProjectDailyCostChargedEvent _)
    {
        RefreshBuff();
    }

}