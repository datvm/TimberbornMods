namespace BuffDebuff;

public abstract class SimpleValueBuff<TValue, TBuff, TInstance>(ISingletonLoader loader, IBuffService buffs)
    : IBuff, ILoadableSingleton, ISaveableSingleton
    where TValue : notnull
    where TBuff : SimpleValueBuff<TValue, TBuff, TInstance>
    where TInstance : BuffInstance<TValue, TBuff>, new()
{
    protected abstract SingletonKey SingletonKey { get; }

    public long Id { get; set; }

    public abstract string Name { get; }
    public abstract string Description { get; }

    public virtual TInstance CreateInstance(TValue value)
    {
        var instance = buffs.CreateBuffInstance<TBuff, TInstance, TValue>((TBuff)this, value);
        return instance;
    }

    public virtual void Load()
    {
        if (loader.HasSingleton(SingletonKey))
        {
            var s = loader.GetSingleton(SingletonKey);
            LoadSingleton(s);
        }

        buffs.Register(this);

        AfterLoad();
    }

    protected virtual void LoadSingleton(IObjectLoader loader)
    {
        Id = loader.GetBuffEntityId();
    }

    protected virtual void AfterLoad() { }

    public virtual void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SingletonKey);
        SaveSingleton(s);
    }

    protected virtual void SaveSingleton(IObjectSaver saver)
    {
        saver.SetBuffEntityId(Id);
    }

}

public abstract class SimpleFloatBuff<TBuff, TInstance>(ISingletonLoader loader, IBuffService buffs)
    : SimpleValueBuff<float, TBuff, TInstance>(loader, buffs)
    where TBuff : SimpleValueBuff<float, TBuff, TInstance>
    where TInstance : BuffInstance<float, TBuff>, new()
{
}