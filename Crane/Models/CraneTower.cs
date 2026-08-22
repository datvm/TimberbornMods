namespace Crane.Models;

public class CraneTower(CraneComponent Crane)
{
    readonly List<ICraneJob> jobs = [];

    public BoundsInt WorkingBounds { get; internal set; }

    public CraneComponent Crane { get; } = Crane;
    public Vector3Int Bottom => Crane.Coordinates;

    public List<CraneSectionComponent> Sections { get; } = [];
    public List<CraneSectionComponent> UnderConstructionSections { get; } = [];
    public IReadOnlyList<ICraneJob> Jobs => jobs;
    public Vector3Int Top => Sections.LastOrDefault()?.Coordinates ?? Bottom;
    public Vector3Int TopIncludingUnfinished => UnderConstructionSections.LastOrDefault()?.Coordinates ?? Top;
    public int Height => Top.z - Bottom.z + 1;
    public int TargetHeight => TopIncludingUnfinished.z - Bottom.z + 1;
    public int HorizontalRange => Crane.HorizontalRange;

    public bool Contains(BlockObject bo)
    {
        foreach (var cell in bo.PositionedBlocks.GetOccupiedCoordinates())
        {
            if (WorkingBounds.Contains(cell))
            {
                return true;
            }
        }

        return false;
    }

    public bool AddJob(ICraneJob job)
    {
        if (jobs.Contains(job))
        {
            return false;
        }

        InsertSorted(job);
        job.PriorityChanged += OnJobPriorityChanged;
        return true;
    }

    public bool RemoveJob(ICraneJob job)
    {
        if (!jobs.Remove(job))
        {
            return false;
        }

        job.PriorityChanged -= OnJobPriorityChanged;
        return true;
    }

    public void ClearJobs()
    {
        foreach (var job in jobs)
        {
            job.PriorityChanged -= OnJobPriorityChanged;
        }

        jobs.Clear();
    }

    void OnJobPriorityChanged(object sender, PriorityChangedEventArgs e)
    {
        if (sender is not ICraneJob job || !jobs.Remove(job))
        {
            return;
        }

        InsertSorted(job);
    }

    void InsertSorted(ICraneJob job)
    {
        for (var i = 0; i < jobs.Count; i++)
        {
            if (CompareJobs(job, jobs[i]) < 0)
            {
                jobs.Insert(i, job);
                return;
            }
        }

        jobs.Add(job);
    }

    static int CompareJobs(ICraneJob left, ICraneJob right)
    {
        var leftSection = left is ConstructionSiteCraneJob { IsMastSection: true };
        var rightSection = right is ConstructionSiteCraneJob { IsMastSection: true };
        if (leftSection != rightSection)
        {
            return leftSection ? -1 : 1;
        }

        var priority = right.Priority.CompareTo(left.Priority);
        if (priority != 0)
        {
            return priority;
        }

        var leftMaterial = left is IMaterialCraneJob;
        var rightMaterial = right is IMaterialCraneJob;
        if (leftMaterial != rightMaterial)
        {
            return leftMaterial ? -1 : 1;
        }

        return 0;
    }

}
