namespace BuildingHP.Components;

public class BuildingHPComponent : BaseComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new("BuildingHP");
    static readonly PropertyKey<int> HPKey = new("HP");
    static readonly PropertyKey<int> DurabilityKey = new("Durability");

#nullable disable
    public BuildingHPComponentSpec Spec { get; private set; }
    EntityService entityService;
#nullable enable
    readonly List<IBuildingDurabilityModifier> modifiers = [];
    public IReadOnlyList<IBuildingDurabilityModifier> Modifiers => modifiers;

    public int Durability { get; private set; }
    public int HP { get; private set; }
    public float HPPercent => Durability > 0 ? (float)HP / Durability : 0f;

    public bool Invulnerable { get; private set; }

    public event Action<BuildingHPComponent>? OnBuildingHPChanged;

    [Inject]
    public void Inject(EntityService entityService)
    {
        this.entityService = entityService;
    }

    public void Awake()
    {
        Spec = GetComponentFast<BuildingHPComponentSpec>();
        InitModifiers();
    }

    public void Start()
    {
        RecalculateDurability();
    }

    public void RecalculateDurability()
    {
        var prev = Durability;
        var invulnerable = false;

        var curr = Spec.BaseDurability;
        foreach (var m in modifiers)
        {
            var delta = m.Delta;
            if (delta.HasValue)
            {
                curr += delta.Value;
            }

            if (m.Invulnerable)
            {
                invulnerable = true;
            }
        }

        foreach (var m in modifiers)
        {
            var mul = m.Multiplier;
            if (mul.HasValue)
            {
                curr = (int)(curr * mul.Value);
            }
        }

        if (prev == curr && invulnerable == Invulnerable) { return; }
        Invulnerable = invulnerable;
        Durability = curr;

        if (prev < curr)
        {
            InternalSetHP(HP + curr - prev);
        }
        else if (HP > curr)
        {
            InternalSetHP(curr);
        }
    }

    public void ChangeHP(int delta)
    {
        if (delta == 0) { return; }

        // Do not damage invulnerable buildings
        if (delta < 0 && Invulnerable) { return; }

        InternalSetHP(Math.Clamp(HP + delta, 0, Durability));
    }

    public void Damage(int damage) => ChangeHP(-damage);

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
            entityService.Delete(this);
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
        RecalculateDurability();
    }

}

public readonly record struct BuildingHPStat(int HP, int Durability)
{

    public BuildingHPStat(BuildingHPComponent comp) : this(comp.HP, comp.Durability) { }

}