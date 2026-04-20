namespace DistroStorage.Components;

public class DefaultDistroSenderTimer : IDistroSenderTimer
{
    static readonly PropertyKey<float> NextTransferKey = new(nameof(NextTransfer));

    readonly Dictionary<string, float> modifiers = [];

    public DefaultDistroSenderTimer(float hoursPerItem)
    {
        BaseHoursPerItem = hoursPerItem;
        Reset();
    }

    public float BaseHoursPerItem { get; }
    public float EffectiveHoursPerItem => BaseHoursPerItem * modifiers.Values.Aggregate(1f, (a, b) => a * b);
    public float NextTransfer { get; set; } 

    public void Reset()
    {
        NextTransfer = EffectiveHoursPerItem;
    }

    public void Save(IObjectSaver s)
    {
        s.Set(NextTransferKey, NextTransfer);
    }

    public void Load(IObjectLoader s)
    {
        NextTransfer = s.Get(NextTransferKey);
    }

    public void SetModifier(string id, float multiplier)
    {
        modifiers[id] = multiplier;

        var newEffective = EffectiveHoursPerItem;
        if (NextTransfer > newEffective)
        {
            NextTransfer = newEffective;
        }
    }

    public void RemoveModifier(string id) => modifiers.Remove(id);
}
