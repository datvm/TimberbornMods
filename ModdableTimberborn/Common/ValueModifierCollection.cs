namespace ModdableTimberborn.Common;

public class ValueModifierCollection<T, TModdableValue, TValue> : ModifierCollection<T>
    where T : IModdableModifier<TModdableValue, TValue>
    where TModdableValue : IModdableValue<TValue>
{

    public ValueModifierCollection(BaseComponent comp) : base(comp)
    {
    }

    public ValueModifierCollection(IEnumerable<T> modifiers) : base(modifiers)
    {
    }

    public void Modify(TModdableValue value) => Modify(value, false);
    public void Modify(TModdableValue value, bool forceDirty)
    {
        if (!forceDirty && !IsDirty) { return; }

        value.Value = value.OriginalValue;
        foreach (var modifier in Modifiers)
        {
            if (modifier.Disabled) { continue; }
            if (modifier.Modify(value)) { break; }
        }

        IsDirty = true;
    }

}