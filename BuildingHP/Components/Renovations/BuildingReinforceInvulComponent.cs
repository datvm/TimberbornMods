namespace BuildingHP.Components.Renovations;

public class BuildingReinforceInvulComponent : BaseComponent, IBuildingInvulnerabilityModifier, IPersistentEntity, IEntityEffectDescriber
{
    static readonly PropertyKey<float> ProgressKey = new("BuildingReinforceInvulComponentInvulnerableTime");

#nullable disable
    ITimeTriggerFactory timeTriggerFactory;
    IDayNightCycle dayNightCycle;
    RenovationSpec spec;
#nullable enable

    ITimeTrigger? remainingTime;
    float? pendingProgress;

    public bool Invulnerable => remainingTime?.InProgress == true;
    public string DescriptionKey { get; } = "LV.BHP.ReinforceInvul";
    public float? ModifierEndTime => remainingTime?.DaysLeft + dayNightCycle.PartialDayNumber;

    public event Action<IBuildingDurabilityModifier>? OnChanged;

    [Inject]
    public void Inject(ITimeTriggerFactory timeTriggerFactory, IDayNightCycle dayNightCycle)
    {
        this.timeTriggerFactory = timeTriggerFactory;
        this.dayNightCycle = dayNightCycle;
    }

    public void Awake()
    {
        var comp = this.GetRenovationComponent();
        comp.RenovationCompleted += OnRenovationCompleted;        
    }

    public void Initialize()
    {
        spec = this.GetRenovationComponent().RenovationService.GetSpec(ReinforceInvulRenovationProvider.RenoId);

        if (pendingProgress.HasValue)
        {
            CreateTimedInvulnerable(pendingProgress.Value);
        }
    }

    void CreateTimedInvulnerable(float? progress = null)
    {
        var comp = this.GetRenovationComponent();
        
        var duration = spec.Parameters[0];
        remainingTime = timeTriggerFactory.Create(OnExpired, duration);
        remainingTime.Resume();

        if (progress.HasValue)
        {
            remainingTime.FastForwardProgress(progress.Value);
        }

        OnChanged?.Invoke(this);
    }

    void OnExpired()
    {
        remainingTime = null;

        var comp = this.GetRenovationComponent();
        comp.RemoveActiveRenovation(ReinforceInvulRenovationProvider.RenoId);

        OnChanged?.Invoke(this);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(BuildingHPComponent.SaveKey, out var s)) { return; }

        if (s.Has(ProgressKey))
        {
            pendingProgress = s.Get(ProgressKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var progress = remainingTime?.Progress;
        if (progress is null || remainingTime!.Finished) { return; }

        var s = entitySaver.GetComponent(BuildingHPComponent.SaveKey);
        s.Set(ProgressKey, progress.Value);
    }

    private void OnRenovationCompleted(BuildingRenovation obj)
    {
        if (obj.Id != ReinforceInvulRenovationProvider.RenoId) { return; }
        CreateTimedInvulnerable();
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle) 
        => Invulnerable ? new(spec.Title.Value, spec.Description, remainingTime!.DaysLeft) : null;

}
