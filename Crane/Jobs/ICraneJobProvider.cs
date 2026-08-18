namespace Crane.Jobs;

public interface ICraneJobProvider
{
    IEnumerable<ICraneJob> GetJobForCrane(CraneComponent crane);

    event EventHandler<ICraneJob>? OnPossibleNewJob;
    event EventHandler<ICraneJob>? OnJobRemoved;
}
