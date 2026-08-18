namespace Crane.Jobs;

[MultiBind(typeof(ICraneJobProvider), AlsoBindSelf = true)]
public class DemolishCraneJobProvider(
    DemolishJobs demolishJobs
) : ICraneJobProvider
{
    public event EventHandler<ICraneJob>? OnPossibleNewJob;
    public event EventHandler<ICraneJob>? OnJobRemoved;

    public IEnumerable<ICraneJob> GetJobForCrane(CraneComponent crane)
    {
        foreach (var priority in Priorities.Ascending)
        {
            foreach (var demolishJob in demolishJobs.GetJobs(priority))
            {
                var job = demolishJob.GetComponent<DemolishableCraneJob>();
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
