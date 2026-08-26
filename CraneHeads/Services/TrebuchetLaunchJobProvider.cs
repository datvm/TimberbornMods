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

        var inventory = attached.GetComponent<CraneHeadTrebuchetInventory>();
        if (inventory && inventory.IsForCrane(crane))
        {
            yield return inventory;
        }
    }

    public void NotifyNew(ICraneJob job) => OnPossibleNewJob?.Invoke(this, job);

    public void NotifyRemoved(ICraneJob job) => OnJobRemoved?.Invoke(this, job);
}
