namespace BuffDebuff;

public interface IBuffInstance<TBuff>
    where TBuff : IBuff
{
    TBuff Buff { get; }
}

public interface IValuedBuffInstance<TValue>
    where TValue : notnull
{
    TValue Value { get; protected internal set; }
}

public abstract class BuffInstance<TValue, TBuff> : ValuedBuffInstance<TValue>, IBuffInstance<TBuff>, IValuedBuffInstance<TValue>
    where TBuff : IBuff
    where TValue : notnull
{
    public new TBuff Buff => (TBuff)base.Buff;
}

public abstract class BuffInstance<TBuff> : BuffInstance, IBuffInstance<TBuff>
    where TBuff : IBuff
{
    public new TBuff Buff => (TBuff)base.Buff;
}

public abstract class ValuedBuffInstance<TValue> : BuffInstance, IValuedBuffInstance<TValue>
    where TValue : notnull
{

    [JsonProperty]
    public TValue Value { get; set; } = default!;

    internal void SetValue(TValue value)
    {
        Value = value;
    }

}