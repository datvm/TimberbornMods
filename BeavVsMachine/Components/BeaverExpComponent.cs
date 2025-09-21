
namespace BeavVsMachine.Components;

public class BeaverExpComponent : BaseComponent, IPersistentEntity, IEntityEffectDescriber
{
    public const float ExpToBonusRatio = 1f / 80f;
    public const string BonusId = "BeaverExpWorkBonus";
    
    static readonly ComponentKey SaveKey = new(nameof(BeaverExpComponent));
    static readonly PropertyKey<float> ExpKey = new("Exp");
    
    public float Experience { get; private set; }
    public int ExperienceBonus => Mathf.FloorToInt(Experience * ExpToBonusRatio);
    public event EventHandler<float>? OnExperienceChanged;
    int currBonus;

    public int Order { get; } = -100;

#nullable disable
    BonusTrackerComponent bonusTracker;
#nullable enable

    public void Awake()
    {
        bonusTracker = this.GetBonusTracker();
    }

    public void Start()
    {
        CheckForExpBonus();
    }

    public void AddExperience(float amount)
    {
        Experience += amount;
        OnExperienceChanged?.Invoke(this, Experience);
        CheckForExpBonus();
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

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(ExpKey))
        {
            Experience = s.Get(ExpKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(ExpKey, Experience);
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle)
        => new(t.T("LV.BVM.ExpTitle"), t.T("LV.BVM.ExpDesc", Experience, currBonus));
}
