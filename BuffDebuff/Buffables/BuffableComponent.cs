namespace BuffDebuff;

public class BuffableComponent : BaseComponent, IBuffEntity, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new("Buffable");

    public long Id { get; set; }

    readonly HashSet<BuffInstance> buffs = [];
    readonly Dictionary<Type, List<BuffInstance>> buffsByTypes = [];
    public IEnumerable<BuffInstance> Buffs => buffs;

    EventBus eventBus = null!;
    IBuffableService buffable = null!;

    [Inject]
    public void Inject(EventBus eventBus, IBuffableService buffable)
    {
        this.eventBus = eventBus;
        this.buffable = buffable;
    }

    public IEnumerable<T> GetBuffs<T>() where T : BuffInstance
    {
        if (!buffsByTypes.TryGetValue(typeof(T), out var list)) { return []; }
        return list.Cast<T>();
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
        if (!buffsByTypes.TryGetValue(type, out var list))
        {
            buffsByTypes[type] = list = [];
        }

        list.Add(buff);

        eventBus.Post(new BuffAddedToEntityEvent(buff, this));
    }

    internal void RemoveBuff(BuffInstance buff)
    {
        if (!buffs.Contains(buff)) { return; }

        buffs.Remove(buff);

        var type = buff.GetType();
        if (!buffsByTypes.TryGetValue(type, out var list)) { return; }

        list.Remove(buff);

        eventBus.Post(new BuffRemovedFromEntityEvent(buff, this));
    }

}
