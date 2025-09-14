namespace ModdableTimberborn.Common;

public interface IModdableValue<TValue>
{
    TValue Value { get; set; }
    TValue OriginalValue { get; }
}

public class ModdableValue<TValue>(TValue original) : IModdableValue<TValue>
{
    public TValue Value { get; set; } = original;
    public TValue OriginalValue { get; protected set; } = original;

    public void Deconstruct(out TValue value, out TValue originalValue)
    {
        value = Value;
        originalValue = OriginalValue;
    }

}

public readonly record struct ModdableValueChanged<TValue>(TValue NewValue, TValue OldValue);