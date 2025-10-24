namespace HydroFormaProjects.Services;

public class SluiceUpstreamService : SimpleProjectListener
{
    public override string ProjectId { get; } = HydroFormaModUtils.SluiceUpgrade;

    public void SyncSluice(SluiceState sluiceState)
    {
        sluiceState.Synchronize();
    }

}
