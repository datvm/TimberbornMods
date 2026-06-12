namespace ModdableTimberborn.Common;

public class StackableValue<T>(T initialValue)
{
    readonly List<StackableModifier<T>> modifiers = [];
    static readonly IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

    public T InitialValue { get; } = initialValue;
    public T CachedValue { get; private set; } = initialValue;
    public event EventHandler<T>? OnValueChanged;

    int calculatedFrame = -1;

    public T CalculateThisFrame() => CalculateThisFrame(InitialValue);
    public T CalculateThisFrame(T initialValue)
    {
        if (calculatedFrame == Time.frameCount)
        {
            return CachedValue;
        }

        calculatedFrame = Time.frameCount;
        return CalculateValue(initialValue);
    }

    public T CalculateValue() => CalculateValue(InitialValue);
    public T CalculateValue(T initialValue)
    {
        if (modifiers.Count == 0)
        {
            SetCachedValue(initialValue);
            return CachedValue;
        }

        var ctx = new StackableValueCalculationContext<T>(initialValue);

        foreach (var m in modifiers)
        {
            m.Process(ctx);

            if (ctx.ShortCircuit)
            {
                break;
            }
        }

        SetCachedValue(ctx.Value);
        return CachedValue;
    }

    public void AddOrUpdate(StackableModifier<T> modifier)
    {
        var index = modifiers.FindIndex(m => m.Id == modifier.Id);
        if (index >= 0)
        {
            modifiers[index] = modifier;
            InvalidateFrameCache();
            modifiers.Sort(CompareModifierPriority);
            return;
        }

        modifiers.Add(modifier);
        InvalidateFrameCache();

        if (modifiers.Count > 1 && modifiers[^2].Priority > modifier.Priority)
        {
            modifiers.Sort(CompareModifierPriority);
        }
    }

    public void Remove(string id)
    {
        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i].Id == id)
            {
                modifiers.RemoveAt(i);
                InvalidateFrameCache();
                break;
            }
        }
    }

    void SetCachedValue(T value)
    {
        if (comparer.Equals(CachedValue, value))
        {
            return;
        }

        CachedValue = value;
        OnValueChanged?.Invoke(this, CachedValue);
    }

    void InvalidateFrameCache() => calculatedFrame = -1;

    static int CompareModifierPriority(StackableModifier<T> a, StackableModifier<T> b)
        => a.Priority.CompareTo(b.Priority);
}

public record StackableModifier<T>(
    string Id,
    int Priority,
    Action<StackableValueCalculationContext<T>> Process
);

public class StackableValueCalculationContext<T>(T initialValue)
{
    public T InitialValue { get; } = initialValue;
    public T Value { get; set; } = initialValue;
    public bool ShortCircuit { get; set; } = false;
}
