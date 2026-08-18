namespace Crane.Jobs;

[MultiBind(typeof(ICraneJobProvider), AlsoBindSelf = true)]
public class ConstructionCraneJobProvider(
    ConstructionRegistry constructionRegistry
) : ICraneJobProvider
{
    public event EventHandler<ICraneJob>? OnPossibleNewJob;

    public event EventHandler<ICraneJob>? OnJobRemoved;

    public IEnumerable<ICraneJob> GetJobForCrane(CraneComponent crane)
    {
        foreach (var priority in Priorities.Ascending)
        {
            foreach (var constructionJob in constructionRegistry.GetJobs(priority))
            {
                var job = constructionJob.GetComponent<ConstructionSiteCraneJob>();
                if (job && job.IsForCrane(crane))
                {
                    yield return job;
                }
            }
        }
    }

    public void NotifyNew(ICraneJob job) => OnPossibleNewJob?.Invoke(this, job);

    public void NotifyRemoved(ICraneJob job) => OnJobRemoved?.Invoke(this, job);
}
