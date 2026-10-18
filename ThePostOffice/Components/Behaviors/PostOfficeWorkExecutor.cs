namespace ThePostOffice.Components.Behaviors;

public class PostOfficeWorkExecutor(
    IDayNightCycle dayNightCycle,
    ReferenceSerializer references
) : BaseComponent, IAwakableComponent, IInitializableEntity, IExecutor
{
    const string BuildingAnimation = "Building";

    static readonly ComponentKey SaveKey = new(nameof(PostOfficeWorkExecutor));
    static readonly PropertyKey<float> FinishTimestampKey = new("FinishTimestamp");
    static readonly PropertyKey<Workplace> WorkplaceKey = new("Workplace");

    Worker worker = null!;
    CharacterAnimator characterAnimator = null!;
    Workplace workplace = null!;
    Workshop? workshop;
    PostOfficeActiveParcel active = null!;
    BlockableObject blockable = null!;
    float finishTimestamp;
    bool isWorking;
    bool loadedWorking;

    public void Awake()
    {
        worker = GetComponent<Worker>();
        characterAnimator = GetComponent<CharacterAnimator>();
        worker.GotUnemployed += OnGotUnemployed;
    }

    public void InitializeEntity()
    {
        if (loadedWorking)
        {
            StartWorking();
        }
    }

    public bool Launch(float maxWorkingTimeInHours)
    {
        Bind(worker.Workplace);
        if (!workplace || !blockable.IsUnblocked || !active.IsRunnable)
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

        if (!blockable.IsUnblocked || !active.IsRunnable)
        {
            StopWorking();
            return ExecutorStatus.Failure;
        }

        active.Progress(deltaTimeInHours * worker.WorkingSpeedMultiplier);
        if (dayNightCycle.PartialDayNumber > finishTimestamp || !active.IsRunnable)
        {
            StopWorking();
            return ExecutorStatus.Success;
        }

        return ExecutorStatus.Running;
    }

    public void Save(IEntitySaver entitySaver)
    {
        var saver = entitySaver.GetComponent(SaveKey);
        saver.Set(FinishTimestampKey, finishTimestamp);
        if (workplace)
        {
            saver.Set(WorkplaceKey, workplace, references.Of<Workplace>());
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var loader))
        {
            return;
        }

        finishTimestamp = loader.Has(FinishTimestampKey) ? loader.Get(FinishTimestampKey) : 0f;
        if (!loader.Has(WorkplaceKey))
        {
            return;
        }

        Bind(loader.Get(WorkplaceKey, references.Of<Workplace>()));
        if (!workplace || !active)
        {
            Clear();
            return;
        }

        loadedWorking = true;
    }

    void Bind(Workplace source)
    {
        if (!source)
        {
            return;
        }

        workplace = source;
        workshop = source.GetComponent<Workshop>();
        active = source.GetComponent<PostOfficeActiveParcel>();
        blockable = source.GetComponent<BlockableObject>();
    }

    void StartWorking()
    {
        var current = workshop;
        if (current)
        {
            current!.InformOfStartedWorking();
        }

        isWorking = true;
        ToggleBuildingAnimation(true);
    }

    void StopWorking()
    {
        var current = workshop;
        if (current)
        {
            current!.InformOfStoppedWorking();
        }

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
        if (isWorking)
        {
            StopWorking();
            Clear();
        }
    }

    void Clear()
    {
        workplace = null!;
        workshop = null;
        active = null!;
        blockable = null!;
        loadedWorking = false;
    }
}
