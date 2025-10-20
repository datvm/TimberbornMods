namespace ModdableTimberborn.Common;

public interface IModifiableEvent<TModifier, TValue>
    where TModifier : IModdableModifier
{
    IReadOnlyList<TModifier> Modifiers { get; }
    TValue OriginalValue { get; }
    TValue CurrentValue { get; set; }
}
