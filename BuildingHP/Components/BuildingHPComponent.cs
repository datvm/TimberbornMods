namespace BuildingHP.Components;

public class BuildingHPComponent : BaseComponent, IPersistentEntity, IEntityDescriber
{
    public static readonly ComponentKey SaveKey = new("BuildingHP");
    static readonly PropertyKey<int> HPKey = new("HP");
    static readonly PropertyKey<int> DurabilityKey = new("Durability");

#nullable disable
    public BuildingHPComponentSpec Spec { get; private set; }
    public BuildingHPService BuildingHPService { get; private set; }
#nullable enable
    readonly List<IBuildingDurabilityModifier> modifiers = [];
    public IReadOnlyList<IBuildingDurabilityModifier> Modifiers => modifiers;

    public ImmutableArray<BuildingDurabilityDescription> DurabilityDescriptions { get; private set; } = [];

    public int Durability { get; private set; }
    public int HP { get; private set; }
    public float HPPercent => Durability > 0 ? (float)HP / Durability : 0f;
    public int HPPercentInt => Durability > 0 ? (HP * 100 / Durability) : 0;

    public bool Invulnerable { get; private set; }
    public event Action<BuildingHPComponent>? OnBuildingHPChanged;

    bool loaded;

    [Inject]
    public void Inject(BuildingHPService buildingHPService)
    {
        this.BuildingHPService = buildingHPService;
    }

    public void Awake()
    {
        Spec = GetComponentFast<BuildingHPComponentSpec>();
        InitModifiers();
    }

    public void Start()
    {
        foreach (var m in Modifiers)
        {
            m.Initialize();
        }

        loaded = true;
        RecalculateDurability();
    }

    public void RecalculateDurability()
    {
        if (!loaded) { return; }

        var prev = Durability;
        var invulnerable = false;

        List<BuildingDurabilityDescription> desc = [
            new("LV.BHP.BaseDurabilityTooltip", Spec.BaseDurability, BuildingDurabilityModifierType.Addition, null)
        ];

        float curr = Spec.BaseDurability;

        foreach (var m in modifiers)
        {
            if (m is IBuildingDeltaDurabilityModifier dm && dm.Delta.HasValue)
            {
                var d = dm.Delta.Value;
                curr += d;
                desc.Add(new(m.DescriptionKey, d, BuildingDurabilityModifierType.Addition, m.ModifierEndTime));
            }

            if (m is IBuildingInvulnerabilityModifier v && v.Invulnerable)
            {
                invulnerable = true;
                desc.Add(new(m.DescriptionKey, 0, BuildingDurabilityModifierType.Invulnerability, m.ModifierEndTime));
            }
        }

        foreach (var m in modifiers)
        {
            if (m is IBuildingMultiplierDurabilityModifier mm && mm.Multiplier.HasValue)
            {
                var mul = mm.Multiplier.Value;
                curr *= mul;
                desc.Add(new(m.DescriptionKey, mul, BuildingDurabilityModifierType.Multiplier, m.ModifierEndTime));
            }
        }
        DurabilityDescriptions = [.. desc];

        if (prev == curr && invulnerable == Invulnerable) { return; }
        Invulnerable = invulnerable;

        var rounded = Mathf.RoundToInt(curr);
        Durability = Mathf.RoundToInt(rounded);

        if (prev < rounded)
        {
            InternalSetHP(HP + rounded - prev);
        }
        else if (HP > rounded)
        {
            InternalSetHP(rounded);
        }

    }

    public void ChangeHP(int delta)
    {
        if (delta == 0) { return; }
        SetHP(HP + delta, false);
    }

    public void SetHP(int hp, bool ignoreInvulnerability = false)
    {
        if (
            hp == HP ||
            (!ignoreInvulnerability && Invulnerable && hp < HP)) { return; }

        InternalSetHP(Math.Clamp(hp, 0, Durability));
    }

    public void Damage(int damage) => ChangeHP(-damage);

    public void Heal(int heal) => ChangeHP(heal);

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(HPKey))
        {
            HP = s.Get(HPKey);
        }

        if (s.Has(DurabilityKey))
        {
            Durability = s.Get(DurabilityKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(HPKey, HP);
        s.Set(DurabilityKey, Durability);
    }

    void InternalSetHP(int hp, bool doNotNotify = false)
    {
        HP = hp;

        if (!doNotNotify)
        {
            OnBuildingHPChanged?.Invoke(this);
        }

        if (hp <= 0)
        {
            BuildingHPService.DestroyBuilding(GetComponentFast<BlockObject>());
        }
    }

    void InitModifiers()
    {
        GetComponentsFast(modifiers);

        foreach (var m in modifiers)
        {
            m.OnChanged += OnModifierChanged;
        }
    }

    void OnModifierChanged(IBuildingDurabilityModifier obj)
    {
        if (!loaded) { return; }
        RecalculateDurability();
    }

    public IEnumerable<EntityDescription> DescribeEntity() 
        => [EntityDescription.CreateTextSection(
            BuildingHPService.BuildingMaterialDurabilityService.GetDisplayedBaseDurability(this), 
            2)];
}

