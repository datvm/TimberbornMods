namespace ModdableWeathers.Common.PrioritizedModifiers;

public abstract class ModdableWeatherPriorityModifierServiceBase<T> where T : IWeatherEntityModifierEntry
{

    public abstract T Default { get; }

    readonly Dictionary<string, T> modifiers;
    public T ActiveEntry { get; private set; }

    public ModdableWeatherPriorityModifierServiceBase()
    {
        var def = Default;

        modifiers = [];
        modifiers.Add(def.Id, def);
        ActiveEntry = def;
    }

    public void AddModifier(T entry)
    {
        modifiers[entry.Id] = entry;
        ActivateNewModifier();
    }

    public void RemoveModifier(string id)
    {
        if (id == Default.Id)
        {
            throw new InvalidOperationException("Cannot remove the default modifier.");
        }

        modifiers.Remove(id);
        ActivateNewModifier();
    }

    protected virtual void ActivateNewModifier()
    {
        var prev = ActiveEntry;
        var highest = FindHighestPriority();

        if (prev.Id == highest.Id) { return; }

        ActiveEntry = highest;
    }

    T FindHighestPriority()
    {
        var result = Default;

        foreach (var entry in modifiers.Values)
        {
            if (entry.Priority > result.Priority)
            {
                result = entry;
            }
        }

        return result;
    }
}