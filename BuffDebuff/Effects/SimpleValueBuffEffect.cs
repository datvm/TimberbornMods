namespace BuffDebuff;

public abstract class SimpleValueBuffEffect<T>(T value) : BaseComponent, IBuffEffect, IPersistentEntity
    where T : notnull
{
    static readonly ComponentKey SaveKey = new("SimpleValue");
    static readonly PropertyKey<string> ValueKey = new("Value");

    public long Id { get; set; }
    public T Value { get; protected set; } = value;
    
    public abstract string Description { get; }

    public void Init() { }
    public void CleanUp() { }

    public void UpdateEffect() { }

    public void Save(IEntitySaver entitySaver)
    {
        entitySaver.GetComponent(SaveKey).Set(ValueKey, Value.ToString());
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.HasComponent(SaveKey)) { return; }
        var str = entityLoader.GetComponent(SaveKey).Get(ValueKey);
        Value = (T)Convert.ChangeType(str, typeof(T));
    }

}
