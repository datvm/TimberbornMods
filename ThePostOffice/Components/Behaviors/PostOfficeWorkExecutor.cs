namespace ThePostOffice.Components.Behaviors;

public class PostOfficeWorkExecutor(
    IDayNightCycle dayNightCycle,
    ReferenceSerializer references
) : BaseComponent, IAwakableComponent, IInitializableEntity, IExecutor
{
    static readonly ComponentKey SaveKey = new(nameof(PostOfficeWorkExecutor));
    static readonly PropertyKey<float> FinishTimestampKey = new("FinishTimestamp");
    static readonly PropertyKey<Workplace> WorkplaceKey = new("Workplace");

    Worker worker = null!;
    Workplace workplace = null!;
    Workshop? workshop;
    IPostOfficeJob job = null!;
    BlockableObject blockable = null!;
    float finishTimestamp;
    bool isWorking;
    bool loadedWorking;

    public void Awake()
    {
        worker = GetComponent<Worker>();
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
        if (!workplace || job is null || !blockable.IsUnblocked || !job.IsRunnable)
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

        if (!blockable.IsUnblocked || job is null || !job.IsRunnable)
        {
            StopWorking(show: true);
            return ExecutorStatus.Failure;
        }

        job.Progress(deltaTimeInHours * worker.WorkingSpeedMultiplier);
        if (dayNightCycle.PartialDayNumber > finishTimestamp || !job.IsRunnable)
        {
            StopWorking(show: !job.IsRunnable);
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
        if (!workplace || job is null)
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
        job = null!;
        if (source.TryGetComponent<PostOfficeSender>(out var sender))
        {
            job = sender;
        }
        else if (source.TryGetComponent<PostOfficeReceiver>(out var receiver))
        {
            job = receiver;
        }

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
        SetVisible(false);
    }

    void StopWorking(bool show)
    {
        var current = workshop;
        if (current)
        {
            current!.InformOfStoppedWorking();
        }

        isWorking = false;
        if (show)
        {
            SetVisible(true);
        }
    }

    void SetVisible(bool visible)
    {
        var model = worker.GetComponent<CharacterModel>();
        if (!model)
        {
            return;
        }

        if (visible)
        {
            model.Show();
        }
        else
        {
            model.Hide();
        }
    }

    void OnGotUnemployed(object sender, EventArgs e)
    {
        if (isWorking)
        {
            StopWorking(show: true);
            Clear();
        }
    }

    void Clear()
    {
        workplace = null!;
        workshop = null;
        job = null!;
        blockable = null!;
        loadedWorking = false;
    }
}
