namespace BeaverChronicles.Services;

[BindSingleton]
public class PermanentNeedBoostService(
    ISingletonLoader loader,
    EventBus eb,
    CharacterStatusHelper characterStatusHelper
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(PermanentNeedBoostService));
    static readonly ListKey<string> NeedIdsKey = new("NeedIds");

    readonly HashSet<string> needIds = [];

    public void AddNeedBoosts(IEnumerable<string> ids) => needIds.UnionWith(ids);

    public void Load()
    {
        LoadSavedData();
        eb.Register(this);
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        if (needIds.Count == 0) { return; }

        singletonSaver.GetSingleton(SaveKey).Set(NeedIdsKey, [.. needIds]);
    }

    [OnEvent]
    public void OnNewDay(CycleDayStartedEvent _)
    {
        if (needIds.Count == 0) { return; }

        characterStatusHelper.BoostAllBeaversNeed(needIds);
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s) || !s.Has(NeedIdsKey)) { return; }

        needIds.UnionWith(s.Get(NeedIdsKey));
    }
}
