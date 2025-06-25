namespace HydroFormaProjects.Services;

public class SluiceUpstreamService(ScientificProjectService projects)
{
    bool canUseSluiceUpstream;
    public bool CanUseSluiceUpstream => canUseSluiceUpstream || ReloadCanUseSluiceUpstream();

    public void SetSluiceUpstream(SluiceUpstreamComponent upstream, SluiceState sluiceState, float upstreamThreshold)
    {
        upstream.Threshold = upstreamThreshold;
    }

    bool ReloadCanUseSluiceUpstream() => canUseSluiceUpstream = projects.IsUnlocked(HydroFormaModUtils.SluiceUpgrade);
}
