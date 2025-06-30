namespace HydroFormaProjects.Services;

public class SluiceUpstreamService(ScientificProjectService projects)
    : BaseProjectService(projects)
{
    protected override string ProjectId { get; } = HydroFormaModUtils.SluiceUpgrade;

    public void SyncSluice(SluiceState sluiceState)
    {
        sluiceState.Synchronize();
    }

}
