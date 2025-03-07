namespace ScientificProjects.Buffs;

public abstract class SimpleValueBuffEffect<TValue> : IBuffEffect
{
    public string? Description { get; }
    public TValue Value { get; protected set; }

    protected abstract string? GetDescription(TValue value);

    public long Id { get; set; }

    public SimpleValueBuffEffect(TValue value)
    {
        Value = value;
        Description = GetDescription(value);
    }

    public void CleanUp() { }
    public void Init() { }
    public void UpdateEffect() { }
}
