namespace BeaverChronicles.Services.Buffs;

public abstract partial class EntityBuffService<T>(
    ISingletonLoader loader,
    IDayNightCycle dayNightCycle
) : ILoadableSingleton, ISaveableSingleton, ITickableSingleton where T : EntityStatus
{
    static readonly PropertyKey<string> BuffsKey = new("Buffs");

    protected EntityBuffBuckets<T> Buffs { get; private set; } = new();

    protected abstract string SaveId { get; }

    public void Load()
    {
        LoadSavedData();
        OnLoaded();
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var state = new EntityBuffState<T>([.. Buffs.Limited.Values], [.. Buffs.Permanent.Values]);
        if (state.IsEmpty) { return; }

        singletonSaver.GetSingleton(new(SaveId)).Set(BuffsKey, JsonConvert.SerializeObject(state));
    }

    public void Tick()
    {
        if (Buffs.Limited.Count == 0) { return; }

        var day = dayNightCycle.PartialDayNumber;
        var expiredIds = Buffs.Limited.Values
            .Where(b => b.UntilDay <= day)
            .Select(b => b.Id)
            .ToArray();

        foreach (var id in expiredIds)
        {
            Remove(id);
        }
    }

    protected void AddOrUpdate(T buff, float? days)
    {
        buff = PrepareBuff(buff, days);

        Remove(buff.Id);
        Buffs[buff.Category][buff.Id] = buff;
        Apply(buff);
    }

    protected void Remove(string buffId)
    {
        if (Buffs.Limited.Remove(buffId, out var limited))
        {
            RemoveBuff(limited);
        }

        if (Buffs.Permanent.Remove(buffId, out var permanent))
        {
            RemoveBuff(permanent);
        }
    }

    protected virtual void OnLoaded() { }
    protected abstract void Apply(T buff);
    protected abstract void RemoveBuff(T buff);

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(new(SaveId), out var s) || !s.Has(BuffsKey)) { return; }

        var state = JsonConvert.DeserializeObject<EntityBuffState<T>>(s.Get(BuffsKey));
        if (state is null) { return; }

        Buffs = new(state.Limited.ToDictionary(b => b.Id), state.Permanent.ToDictionary(b => b.Id));
    }

    T PrepareBuff(T buff, float? days)
        => buff.Category == EntityBuffCategory.Permanent || days is null
            ? buff
            : (T)buff.WithUntilDay(dayNightCycle.PartialDayNumber + days.Value);

}

record EntityBuffState<TBuff>(TBuff[] Limited, TBuff[] Permanent) where TBuff : EntityStatus
{
    [JsonIgnore]
    public bool IsEmpty => Limited.Length == 0 && Permanent.Length == 0;
}
