namespace BuffDebuff;

public abstract class BuffInstance<TBuff>(TBuff buff) : BuffInstance(buff) where TBuff : IBuff
{
    public new TBuff Buff => (TBuff)base.Buff;
}

public abstract class BuffInstance(IBuff buff) : BaseComponent, IBuffEntity, IPersistentEntity
{
    protected static readonly ComponentKey SaveKey = new("BuffInstance");
    static readonly PropertyKey<bool> ActiveKey = new("Active");

    public long Id { get; set; }

    public IBuff Buff { get; init; } = buff;
    public abstract bool IsBuff { get; protected set; }
    public bool IsDebuff => !IsBuff;

    public bool Active { get; internal set; }

    public abstract IEnumerable<IBuffTarget> Targets { get; }
    public abstract IEnumerable<IBuffEffect> Effects { get; }

    public virtual void Init() { }
    public virtual void CleanUp() { }

    public virtual void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.HasComponent(SaveKey)) { return; }

        var s = entityLoader.GetComponent(SaveKey);
        LoadInstance(s);
    }

    protected virtual void LoadInstance(IObjectLoader loader)
    {
        Active = loader.Get(ActiveKey);
    }

    public virtual void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        SaveInstance(s);
    }

    protected virtual void SaveInstance(IObjectSaver saver)
    {
        saver.SetBuffEntityId(Id);
        saver.Set(ActiveKey, Active);
    }

}
