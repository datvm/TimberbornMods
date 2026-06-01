namespace BeaverChronicles.Services;

[BindSingleton]
public class ChronicleEventFlagService(ISingletonLoader loader) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(ChronicleEventFlagService));
    static readonly ListKey<string> FlagsKey = new("Flags");

    readonly HashSet<string> flags = [with(StringComparer.InvariantCultureIgnoreCase)];
    public ReadOnlyHashSet<string> Flags => new(flags);

    public bool HasFlag(string flag) => flags.Contains(flag);
    public void AddFlag(string flag) => flags.Add(flag);
    public void RemoveFlag(string flag) => flags.Remove(flag);

    public void Load()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(FlagsKey))
        {
            flags.UnionWith(s.Get(FlagsKey));
        }        
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        if (flags.Count > 0)
        {
            s.Set(FlagsKey, flags);
        }
    }

}
