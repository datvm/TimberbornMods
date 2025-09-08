﻿namespace ModdableTimberborn.Common;

public static class ModifierCollection
{

    public static IEnumerable<TModifier> GetModifiersFromComponents<TModifier>(BaseComponent comp)
    {
        List<TModifier> modifiers = [];
        comp.GetComponentsFast(modifiers);
        return modifiers;
    }

}

public class ModifierCollection<T> : IDisposable
    where T : IModdableModifier
{

    public ImmutableArray<T> Modifiers { get; }
    public bool IsDirty { get; set; }

    public ModifierCollection(BaseComponent comp) : this(ModifierCollection.GetModifiersFromComponents<T>(comp))
    { }

    public ModifierCollection(IEnumerable<T> modifiers)
    {
        Modifiers = [.. modifiers.OrderBy(q => q.Priority)];

        foreach (var modifier in Modifiers)
        {
            modifier.OnChanged += MarkDirty;
        }
    }

    public void MarkDirty() => IsDirty = true;

    public void Dispose()
    {
        foreach (var modifier in Modifiers)
        {
            modifier.OnChanged -= MarkDirty;
        }
    }
}
