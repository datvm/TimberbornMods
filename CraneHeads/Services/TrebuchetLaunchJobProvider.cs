namespace CraneHeads.Services;

[MultiBind(typeof(ICraneJobProvider), AlsoBindSelf = true)]
public class TrebuchetLaunchJobProvider : ICraneJobProvider
{
    public event EventHandler<ICraneJob>? OnPossibleNewJob;
    public event EventHandler<ICraneJob>? OnJobRemoved;

    public IEnumerable<ICraneJob> GetJobForCrane(CraneComponent crane)
    {
        var head = crane.GetComponent<CraneTowerHead>()?.Head;
        if (head is not { } attached || !attached)
        {
            yield break;
        }

        var trebuchet = attached.GetComponent<CraneHeadTrebuchet>();
        if (trebuchet && trebuchet.IsForCrane(crane))
        {
            yield return trebuchet;
        }
    }

    public void NotifyNew(ICraneJob job) => OnPossibleNewJob?.Invoke(this, job);

    public void NotifyRemoved(ICraneJob job) => OnJobRemoved?.Invoke(this, job);
}
