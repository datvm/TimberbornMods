namespace EarthquakeWeather.Components;

public class EarthquakeReinforcementComponent : BaseComponent, IPersistentEntity, IBuildingMultiplierDurabilityModifier, IEntityMultiEffectsDescriber
{
    static readonly ComponentKey SaveKey = new(nameof(EarthquakeReinforcementComponent));
    static readonly PropertyKey<int> ImmunityCountKey = new("ImmunityCount");
    static readonly PropertyKey<bool> HasSymbiosisDurabilityKey = new("HasSymbiosisDurability");

    public bool HasDamageReduction { get; private set; }
    public int ImmunityCount { get; private set; }
    public bool HasDormantSymbiosis { get; private set; }
    public bool HasActiveSymbiosis => Multiplier.HasValue;

    public float? Multiplier { get; private set; }
    public string DescriptionKey { get; } = "LV.EQ.EqDurability";
    public float? ModifierEndTime { get; }


#nullable disable
    RenovationSpec damageReductionSpec, damageImmunitySpec, durabilitySpec;

    public event Action<IBuildingDurabilityModifier> OnChanged;
#nullable enable

    public void Awake()
    {
        var reno = this.GetRenovationComponent();
        reno.RenovationCompleted += OnRenovationCompleted;

        var eq = GetComponentFast<EarthquakeComponent>();
        eq.OnBeforeEarthquakeDamage += BeforeEqDamage;
    }

    private void BeforeEqDamage(object sender, EarthquakeBuildingDamageEventArgs e)
    {
        if (HasDormantSymbiosis)
        {
            e.Cancel = true;
            ActivateDormantSymbiosis();
        }
        else if (ImmunityCount > 0)
        {
            e.Cancel = true;
            ActivateImmunity();
        }
        else if (HasDamageReduction)
        {
            e.Damage += Mathf.RoundToInt(e.Damage * damageReductionSpec.Parameters[0]);
        }
    }

    void ActivateImmunity()
    {
        ImmunityCount--;

        if (ImmunityCount <= 0)
        {
            var reno = this.GetRenovationComponent();
            reno.RemoveActiveRenovation(EqImmunityProvider.RenoId);
        }
    }

    void ActivateDormantSymbiosis()
    {
        SetDurability();
        HasDormantSymbiosis = false;
    }

    public void Initialize()
    {
        var reno = this.GetRenovationComponent();
        var serv = reno.RenovationService;

        damageReductionSpec = serv.GetSpec(EqDamageReductionProvider.RenoId);
        damageImmunitySpec = serv.GetSpec(EqImmunityProvider.RenoId);
        durabilitySpec = serv.GetSpec(EqDurabilityProvider.RenoId);

        HasDormantSymbiosis = !HasActiveSymbiosis && reno.HasRenovation(EqDurabilityProvider.RenoId);

        if (reno.HasRenovation(EqDamageReductionProvider.RenoId))
        {
            HasDamageReduction = true;
        }

        if (HasActiveSymbiosis)
        {
            SetDurability();
        }
    }

    private void OnRenovationCompleted(BuildingRenovation obj)
    {
        switch (obj.Id)
        {
            case EqDamageReductionProvider.RenoId:
                HasDamageReduction = true;
                break;
            case EqImmunityProvider.RenoId:
                ImmunityCount = Math.Max(0, ImmunityCount) + (int)damageImmunitySpec.Parameters[0];
                break;
            case EqDurabilityProvider.RenoId:
                HasDormantSymbiosis = true;
                break;
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(ImmunityCountKey))
        {
            ImmunityCount = s.Get(ImmunityCountKey);
        }

        if (s.Has(HasSymbiosisDurabilityKey))
        {
            Multiplier = 1; // Assign later
            HasDormantSymbiosis = false; // Just in case it's somehow set first
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(ImmunityCountKey, ImmunityCount);

        if (HasActiveSymbiosis)
        {
            s.Set(HasSymbiosisDurabilityKey, true);
        }
    }

    void SetDurability()
    {
        if (durabilitySpec is null) { return; }

        Multiplier = durabilitySpec.Parameters[0] + 1f;
        OnChanged?.Invoke(this);
    }

    public IEnumerable<EntityEffectDescription> DescribeAll(ILoc t, IDayNightCycle dayNightCycle)
    {
        if (HasDamageReduction)
        {
            yield return new(damageReductionSpec.Title.Value, damageReductionSpec.Description);
        }

        if (ImmunityCount > 0)
        {
            yield return new(damageImmunitySpec.Title.Value, t.T("LV.EQ.EqImmunityStatus", ImmunityCount));
        }

        if (HasActiveSymbiosis)
        {
            yield return new(
                t.T("LV.EQ.EqDurabilityActive", durabilitySpec.Title.Value),
                t.T("LV.EQ.EqDurabilityActiveDesc", durabilitySpec.Parameters[0]));
        }
        else if (HasDormantSymbiosis)
        {
            yield return new(t.T("LV.EQ.EqDurabilityDormant", durabilitySpec.Title.Value), durabilitySpec.Description);
        }

    }


}
