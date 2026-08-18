namespace Crane.Components.Behaviors;

public class CraneWorkExecutor(
    IDayNightCycle dayNightCycle,
    ReferenceSerializer referenceSerializer
) : BaseComponent, IAwakableComponent, IInitializableEntity, IExecutor
{
    const string BuildingAnimation = "Building";

    static readonly ComponentKey CraneWorkExecutorKey = new("CraneWorkExecutor");
    static readonly PropertyKey<float> FinishTimestampKey = new("FinishTimestamp");
    static readonly PropertyKey<Workplace> WorkplaceKey = new("Workplace");

    Worker worker = null!;
    CharacterAnimator characterAnimator = null!;
    Workplace workplace = null!;
    Workshop workshop = null!;
    CraneWorkshop craneWorkshop = null!;
    BlockableObject blockable = null!;
    float finishTimestamp;
    bool isWorking;

    public void Awake()
    {
        worker = GetComponent<Worker>();
        characterAnimator = GetComponent<CharacterAnimator>();
        worker.GotUnemployed += OnGotUnemployed;
    }

    public void InitializeEntity()
    {
        if (workshop)
        {
            StartWorking();
        }
    }

    public bool Launch(float maxWorkingTimeInHours)
    {
        Bind(worker.Workplace);
        if (!workplace || !blockable.IsUnblocked)
        {
            return false;
        }

        if (!craneWorkshop || !craneWorkshop.HasWork)
        {
            return false;
        }

        finishTimestamp = dayNightCycle.DayNumberHoursFromNow(maxWorkingTimeInHours);
        StartWorking();
        return true;
    }

    public ExecutorStatus Tick(float deltaTimeInHours)
    {
        if (!workplace || !isWorking)
        {
            return ExecutorStatus.Failure;
        }

        if (!blockable.IsUnblocked)
        {
            StopWorking();
            return ExecutorStatus.Failure;
        }

        if (dayNightCycle.PartialDayNumber > finishTimestamp || !craneWorkshop.TryWork(deltaTimeInHours * worker.WorkingSpeedMultiplier * CraneWorkshop.WorkSpeedBonus))
        {
            StopWorking();
            return ExecutorStatus.Success;
        }

        return ExecutorStatus.Running;
    }

    public void Save(IEntitySaver entitySaver)
    {
        var saver = entitySaver.GetComponent(CraneWorkExecutorKey);
        saver.Set(FinishTimestampKey, finishTimestamp);
        if ((bool)workplace)
        {
            saver.Set(WorkplaceKey, workplace, referenceSerializer.Of<Workplace>());
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        var loader = entityLoader.GetComponent(CraneWorkExecutorKey);
        finishTimestamp = loader.Get(FinishTimestampKey);
        var loaded = loader.Has(WorkplaceKey)
            ? loader.Get(WorkplaceKey, referenceSerializer.Of<Workplace>())
            : worker.Workplace;
        Bind(loaded);
        if (!workplace || !workshop || !craneWorkshop)
        {
            Clear();
        }
    }

    void Bind(Workplace source)
    {
        if (!source)
        {
            return;
        }

        workplace = source;
        workshop = source.GetComponent<Workshop>();
        craneWorkshop = source.GetComponent<CraneWorkshop>();
        blockable = source.GetComponent<BlockableObject>();
    }

    void StartWorking()
    {
        workshop.InformOfStartedWorking();
        isWorking = true;
        ToggleBuildingAnimation(true);
    }

    void StopWorking()
    {
        workshop.InformOfStoppedWorking();
        isWorking = false;
        ToggleBuildingAnimation(false);
    }

    void ToggleBuildingAnimation(bool value)
    {
        if (characterAnimator && characterAnimator.HasParameter(BuildingAnimation))
        {
            characterAnimator.SetBool(BuildingAnimation, value);
        }
    }

    void OnGotUnemployed(object sender, EventArgs e)
    {
        if (workshop && isWorking)
        {
            StopWorking();
            Clear();
        }
    }

    void Clear()
    {
        workplace = null!;
        workshop = null!;
        craneWorkshop = null!;
        blockable = null!;
    }

}
