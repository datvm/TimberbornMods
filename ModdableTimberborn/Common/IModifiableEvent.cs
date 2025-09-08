namespace ModdableTimberborn.Common;

public interface IModifiableEvent<TModifier, TValue>
    where TModifier : IModdableModifier
{
    public IReadOnlyList<TModifier> Modifiers { get; }
    public TValue OriginalValue { get; }
    public TValue CurrentValue { get; set; }
}
