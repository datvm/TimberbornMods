namespace BeaverChronicles.Services;

[BindSingleton]
public class CharacterStatusHelper(
    ISingletonLoader loader,
    EventBus eb,
    DefaultEntityTracker<Beaver> beavers,
    DefaultEntityTracker<Bot> bots,
    BeaverPopulation beaverPopulation,
    IDayNightCycle dayNightCycle
) : ILoadableSingleton, ISaveableSingleton, ITickableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(CharacterStatusHelper));
    static readonly ListKey<string> PermanentBoostsKey = new("PermanentBoosts");
    static readonly PropertyKey<string> LimitedTimeStatusKey = new("LimitedTimeStatus");

    public HashSet<string> PermanentNeedBoost { get; } = [];

    Dictionary<string, LimitedTimeCharacterStatus> limitedTimeStatuses = [];
    public IReadOnlyDictionary<string, LimitedTimeCharacterStatus> LimitedTimeStatuses => limitedTimeStatuses;

    public void Load()
    {
        LoadSavedData();
        eb.Register(this);

        beavers.OnEntityRegistered += OnCharacterRegistered;
        bots.OnEntityRegistered += OnCharacterRegistered;
    }

    void OnCharacterRegistered(BaseComponent c)
    {
        if (limitedTimeStatuses.Count == 0) { return; }

        var type = c.GetCharacterType();
        if (type == CharacterType.Unknown) { return; }

        var bonusTracker = c.GetBonusTracker();
        foreach (var status in limitedTimeStatuses.Values)
        {
            if (status.CharacterType.HasFlag(type))
            {
                ApplyBonus(bonusTracker, status);
            }
        }
    }

    public void BoostAllBeaversNeed(IReadOnlyCollection<string> ids, float? amount = null)
    {
        foreach (var b in beavers.Entities)
        {
            foreach (var id in ids)
            {
                BoostNeed(b.GetComponent<NeedManager>(), id, amount);
            }
        }
    }

    public void BoostNeed(NeedManager needMan, string id, float? amount = null)
    {
        if (amount is null)
        {
            var n = needMan.GetNeed(id);
            if (n is null) { return; }

            amount = n.PointsToMax;
        }

        needMan.ApplyEffect(new(id, amount.Value, 1));
    }

    public void AddOrUpdateLimitedTimeBonus(LimitedTimeCharacterStatus status, float? days = null)
    {
        if (days is not null)
        {
            status = status with { UntilDay = dayNightCycle.PartialDayNumber + days.Value };
        }

        if (limitedTimeStatuses.ContainsKey(status.Id))
        {
            RemoveLimitedTimeStatus(status.Id);
        }

        limitedTimeStatuses[status.Id] = status;

        foreach (var entity in GetBonusTrackers(status.CharacterType))
        {
            ApplyBonus(entity, status);
        }
    }

    public void RemoveLimitedTimeStatus(string id)
    {
        if (!limitedTimeStatuses.TryGetValue(id, out var status)) { return; }

        limitedTimeStatuses.Remove(id);
        foreach (var entity in GetBonusTrackers(status.CharacterType))
        {
            entity.Remove(id);
            entity.GetStatusDescription().RemoveStatus(id);
        }
    }

    IEnumerable<BonusTrackerComponent> GetBonusTrackers(CharacterType c)
    {
        var adult = c.HasFlag(CharacterType.AdultBeaver);
        var child = c.HasFlag(CharacterType.ChildBeaver);
        var bot = c.HasFlag(CharacterType.Bot);

        if (adult)
        {
            foreach (var b in beaverPopulation._beaverCollection.Adults)
            {
                yield return b.GetBonusTracker();
            }
        }

        if (child)
        {
            foreach (var b in beaverPopulation._beaverCollection.Children)
            {
                yield return b.GetBonusTracker();
            }
        }

        if (bot)
        {
            foreach (var b in bots.Entities)
            {
                yield return b.GetBonusTracker();
            }
        }
    }

    void ApplyBonus(BonusTrackerComponent comp, LimitedTimeCharacterStatus status)
    {
        comp.AddOrUpdate(new(status.Id, [..status.Bonuses.Select(b => new BonusSpec() {
            Id = b.Bonus,
            MultiplierDelta = b.Amount,
        })]));
        comp.GetStatusDescription().AddStatus(status);
    }

    [OnEvent]
    public void OnNewDay(CycleDayStartedEvent _)
    {
        if (PermanentNeedBoost.Count == 0) { return; }
        BoostAllBeaversNeed(PermanentNeedBoost);
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(PermanentBoostsKey))
        {
            PermanentNeedBoost.UnionWith(s.Get(PermanentBoostsKey));
        }

        if (s.Has(LimitedTimeStatusKey))
        {
            var json = s.Get(LimitedTimeStatusKey);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, LimitedTimeCharacterStatus>>(json);

            if (dict is not null)
            {
                limitedTimeStatuses = dict;
            }
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        if (PermanentNeedBoost.Count > 0)
        {
            s.Set(PermanentBoostsKey, [.. PermanentNeedBoost]);
        }

        if (limitedTimeStatuses.Count > 0)
        {
            s.Set(LimitedTimeStatusKey, JsonConvert.SerializeObject(limitedTimeStatuses));
        }
    }

    public void Tick()
    {
        if (limitedTimeStatuses.Count == 0) { return; }

        var day = dayNightCycle.PartialDayNumber;
        List<string> removingIds = [];

        foreach (var status in limitedTimeStatuses.Values)
        {
            if (status.UntilDay <= day)
            {
                removingIds.Add(status.Id);
            }
        }

        if (removingIds.Count == 0) { return; }
        foreach (var id in removingIds)
        {
            RemoveLimitedTimeStatus(id);
        }
    }

}
