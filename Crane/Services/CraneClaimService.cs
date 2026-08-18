namespace Crane.Services;

[BindSingleton]
public class CraneClaimService(
    IEnumerable<ICraneJobProvider> jobProviders,
    DefaultEntityTracker<CraneComponent> cranes,
    CraneStructureService structureService
) : ILoadableSingleton, IUnloadableSingleton
{
    readonly ICraneJobProvider[] providers = [.. jobProviders];

    public void Load()
    {
        foreach (var provider in providers)
        {
            provider.OnPossibleNewJob += OnPossibleNewJob;
            provider.OnJobRemoved += OnJobRemoved;
        }

        structureService.OnCraneTowerChanged += OnCraneTowerChanged;
    }

    public void Unload()
    {
        foreach (var provider in providers)
        {
            provider.OnPossibleNewJob -= OnPossibleNewJob;
            provider.OnJobRemoved -= OnJobRemoved;
        }

        structureService.OnCraneTowerChanged -= OnCraneTowerChanged;
    }

    public void Refresh(CraneComponent c)
    {
        if (IsEligible(c))
        {
            Rebuild(c);
        }
        else
        {
            c.Tower.ClearJobs();
        }

        NotifyInventory(c);
    }

    void OnCraneTowerChanged(object sender, CraneTower tower) => Refresh(tower.Crane);

    void OnPossibleNewJob(object sender, ICraneJob job)
    {
        foreach (var c in cranes.Entities)
        {
            if (IsEligible(c) && job.IsForCrane(c) && c.Tower.AddJob(job))
            {
                NotifyInventory(c);
            }
        }
    }

    void OnJobRemoved(object sender, ICraneJob job)
    {
        foreach (var c in cranes.Entities)
        {
            if (c.Tower.RemoveJob(job))
            {
                NotifyInventory(c);
            }
        }
    }

    void Rebuild(CraneComponent c)
    {
        c.Tower.ClearJobs();
        foreach (var provider in providers)
        {
            foreach (var job in provider.GetJobForCrane(c))
            {
                c.Tower.AddJob(job);
            }
        }
    }

    static void NotifyInventory(CraneComponent c)
    {
        if (c.GetComponent<CraneInventory>() is { } inventory && inventory)
        {
            inventory.OnJobsChanged();
        }
    }

    static bool IsEligible(CraneComponent c)
    {
        if (!c || !c.IsFinished)
        {
            return false;
        }

        if (c.GetComponent<PausableBuilding>() is { } pausable && pausable && pausable.Paused)
        {
            return false;
        }

        if (c.GetComponent<Demolishable>() is { } demolishable && demolishable && demolishable.IsMarked)
        {
            return false;
        }

        return true;
    }
}
