namespace BeavVsMachine.Components;

public class BeaverExpComponent : BaseComponent, IPersistentEntity, IEntityEffectDescriber, IInitializableEntity, IDeletableEntity, IChildhoodInfluenced
{
    public const float ExpToBonusRatio = 1f / 80f;
    public const string BonusId = "BeaverExpWorkBonus";

    static readonly ComponentKey SaveKey = new(nameof(BeaverExpComponent));
    static readonly PropertyKey<float> ExpKey = new("Exp");
    static readonly PropertyKey<float> PendingWorkExpKey = new("PendingWorkExp");

    public float Experience { get; private set; }
    public int ExperienceBonus => Mathf.FloorToInt(Experience * ExpToBonusRatio);
    public event EventHandler<float>? OnExperienceChanged;

    int currBonus;
    float pendingWorkExp;

    public int Order { get; } = -100;

#nullable disable
    BonusTrackerComponent bonusTracker;
    BeaverExpStatTracker expStatTracker;
#nullable enable

    [Inject]
    public void Inject(BeaverExpStatTracker expStatTracker)
    {
        this.expStatTracker = expStatTracker;
    }

    public void Awake()
    {
        bonusTracker = this.GetBonusTracker();
    }

    public void Start()
    {
        CheckForExpBonus();
    }

    public float AddExperience(float amount) => SetExperience(Experience + amount);

    /// <summary>
    /// Should not be called outside of Dev Tools. Use <see cref="AddExperience(float)"/> instead.
    /// </summary>
    internal float SetExperience(float amount)
    {
        Experience = amount;
        OnExperienceChanged?.Invoke(this, Experience);
        CheckForExpBonus();

        return Experience;
    }

    void CheckForExpBonus()
    {
        var bonus = ExperienceBonus;
        if (bonus == currBonus) { return; }

        currBonus = bonus;
        if (bonus > 0)
        {
            bonusTracker.AddOrUpdate(new(BonusId, BonusType.WorkingSpeed, bonus / 100f));
        }
    }

    public void AddPendingWorkExp(float amount) => pendingWorkExp += amount;

    public float UpdateDayWorkExp()
    {
        if (pendingWorkExp <= 0) { return Experience; }

        var xp = AddExperience(pendingWorkExp);
        pendingWorkExp = 0;

        return xp;
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(ExpKey))
        {
            Experience = s.Get(ExpKey);
        }

        if (s.Has(PendingWorkExpKey))
        {
            pendingWorkExp = s.Get(PendingWorkExpKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(ExpKey, Experience);
        s.Set(PendingWorkExpKey, pendingWorkExp);
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle)
        => new(
            t.T("LV.BVM.ExpTitle", Experience),
            t.T(pendingWorkExp > 0 ? "LV.BVM.ExpDescWithPending" : "LV.BVM.ExpDesc", currBonus, pendingWorkExp));

    public void DeleteEntity()
    {
        expStatTracker.UnregisterBeaver(this);
    }

    public void InitializeEntity()
    {
        expStatTracker.RegisterBeaver(this);
    }

    public void InfluenceByChildhood(Character child)
    {
        var exp = child.GetComponentFast<BeaverExpComponent>();
        if (!exp) { return; }

        SetExperience(exp.Experience);
    }
}
