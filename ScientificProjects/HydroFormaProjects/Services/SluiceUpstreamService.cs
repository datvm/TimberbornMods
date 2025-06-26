namespace HydroFormaProjects.Services;

public class SluiceUpstreamService(ScientificProjectService projects)
{
    bool canUseSluiceUpstream;
    public bool CanUseSluiceUpstream => canUseSluiceUpstream || ReloadCanUseSluiceUpstream();

    public void SyncSluice(SluiceState sluiceState)
    {
        sluiceState.Synchronize();
    }

    bool ReloadCanUseSluiceUpstream() => canUseSluiceUpstream = projects.IsUnlocked(HydroFormaModUtils.SluiceUpgrade);
}
