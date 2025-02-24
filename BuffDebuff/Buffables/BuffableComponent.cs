namespace BuffDebuff;

public class BuffableComponent : BaseComponent, IBuffEntity, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new("Buffable");

    public long Id { get; set; }

    readonly HashSet<BuffInstance> buffs = [];
    readonly Dictionary<Type, List<BuffInstance>> instancesByBuffTypes = [];
    public IEnumerable<BuffInstance> Buffs => buffs;

    EventBus eventBus = null!;
    IBuffableService buffable = null!;

    public event EventHandler<BuffInstance> OnBuffAdded = delegate { };
    public event EventHandler<BuffInstance> OnBuffRemoved = delegate { };
    public event EventHandler<BuffInstance> OnBuffActiveChanged = delegate { };

    [Inject]
    public void Inject(EventBus eventBus, IBuffableService buffable)
    {
        this.eventBus = eventBus;
        this.buffable = buffable;
    }

    public IEnumerable<T> GetBuffInstances<T>(bool activeOnly = true) where T : BuffInstance
    {
        if (!instancesByBuffTypes.TryGetValue(typeof(T), out var list)) { return []; }
        return [..list
            .Where(b => !activeOnly || b.Active)
            .Cast<T>()];
    }

    public IEnumerable<T> GetEffects<T>(bool includeDisabled = false) where T : IBuffEffect
    {
        return buffs
            .Where(b => includeDisabled || b.Active)
            .SelectMany(b => b.Effects)
            .OfType<T>();
    }

    public void Start()
    {
        buffable.Register(this);
    }

    public void Load(IEntityLoader entityLoader)
    {
        LoadId(entityLoader);
    }

    void LoadId(IEntityLoader entityLoader)
    {
        if (!entityLoader.HasComponent(SaveKey)) { return; }

        Id = entityLoader.GetComponent(SaveKey).GetBuffEntityId();
    }

    public void Save(IEntitySaver entitySaver)
    {
        entitySaver.GetComponent(SaveKey).SetBuffEntityId(Id);
    }

    internal void ApplyBuff(BuffInstance buff)
    {
        if (buffs.Contains(buff)) { return; }

        buffs.Add(buff);

        var type = buff.GetType();
        if (!instancesByBuffTypes.TryGetValue(type, out var list))
        {
            instancesByBuffTypes[type] = list = [];
        }

        list.Add(buff);

        eventBus.Post(new BuffAddedToEntityEvent(buff, this));
        OnBuffAdded(this, buff);
    }

    internal void RemoveBuff(BuffInstance buff)
    {
        if (!buffs.Contains(buff)) { return; }

        buffs.Remove(buff);

        var type = buff.GetType();
        if (!instancesByBuffTypes.TryGetValue(type, out var list)) { return; }

        list.Remove(buff);

        eventBus.Post(new BuffRemovedFromEntityEvent(buff, this));
        OnBuffRemoved(this, buff);
    }

    internal void BuffActiveChanged(BuffInstance buff)
    {
        if (!Buffs.Contains(buff)) { return; }

        OnBuffActiveChanged(this, buff);
    }

}
