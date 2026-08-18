namespace Crane.Components;

[AddTemplateModule2(typeof(CraneComponent))]
public class CraneWorkshop : BaseComponent, IAwakableComponent
{
    public const float WorkSpeedBonus = 1.33f;

    CraneComponent crane = null!;
    Emptiable? emptiable;

    public bool IsEmptying => emptiable && emptiable!.IsMarkedForEmptying;

    public bool HasWork => PickJob() is not null;

    public void Awake()
    {
        crane = GetComponent<CraneComponent>();
        emptiable = GetComponent<Emptiable>();
    }

    public bool TryWork(float hours)
    {
        var job = PickJob();
        if (job is null)
        {
            return false;
        }

        job.ProgressJob(crane, hours);
        return true;
    }

    ICraneJob? PickJob()
    {
        foreach (var job in crane.Tower.Jobs)
        {
            if (CanProgress(job))
            {
                return job;
            }
        }

        return null;
    }

    static bool CanProgress(ICraneJob job)
    {
        if (!job.IsAvailable)
        {
            return false;
        }

        return job switch
        {
            ConstructionSiteCraneJob construction => construction.CanHammer,
            _ => true,
        };
    }

}
