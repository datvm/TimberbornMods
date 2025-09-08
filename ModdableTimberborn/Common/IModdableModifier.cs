namespace ModdableTimberborn.Common;

public interface IModdableModifier
{
    string Id { get; }
    int Priority { get; }
    bool Disabled { get; }

    event Action? OnChanged;
}

public interface IModdableModifier<TModdableValue, TValue> : IModdableModifier
    where TModdableValue : IModdableValue<TValue>
{
    bool Modify(TValue value);
}