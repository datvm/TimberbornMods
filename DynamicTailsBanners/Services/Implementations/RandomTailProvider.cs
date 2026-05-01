namespace DynamicTailsBanners.Services.Implementations;

[MultiBind(typeof(IDynamicDecalProvider), AlsoBindSelf = true)]
public class RandomTailProvider(
    ISingletonLoader loader,
    IDecalService decalService,
    EventBus eb
) : IDynamicTailDecalProvider, ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(RandomTailProvider));
    static readonly PropertyKey<bool> SameForAllKey = new("SameForAll");
    static readonly ListKey<string> DecalIdsKey = new("DecalIds");
    static readonly PropertyKey<string> TodayDecalKey = new("TodayDecal");
    static readonly PropertyKey<int> TodayDayKey = new("TodayDay");

    public const string Id = "dynamic-tail-random";
    string IDynamicDecalProvider.Id => Id;

    public bool SameForAll { get; set; }
    public List<string> DecalIds { get; } = [];

    string? todayDecal;
    int todayDay = -1;

    string? currTextureId;
    Texture2D? todayTexture;

    readonly HashSet<DynamicTailDecalApplier> comps = [];

    public Texture2D GetTexture(DynamicTailDecalApplier comp)
    {
        if (!SameForAll)
        {
            return GetTextureFrom(RandomizeDecalId());
        }

        if (currTextureId != todayDecal || !todayTexture)
        {
            currTextureId = todayDecal;
            todayTexture = GetTextureFrom(currTextureId);
        }

        return todayTexture;
    }

    public void Load()
    {
        LoadSavedData();
        eb.Register(this);
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }
        SameForAll = s.Get(SameForAllKey);
        DecalIds.AddRange(s.Get(DecalIdsKey));

        if (s.Has(TodayDecalKey) && s.Has(TodayDayKey))
        {
            todayDecal = s.Get(TodayDecalKey);
            todayDay = s.Get(TodayDayKey);
        }
    }

    public void Register(DynamicTailDecalApplier comp) => comps.Add(comp);
    public void Unregister(DynamicTailDecalApplier comp) => comps.Remove(comp);

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(SameForAllKey, SameForAll);
        s.Set(DecalIdsKey, DecalIds);

        if (todayDecal is not null)
        {
            s.Set(TodayDecalKey, todayDecal);
            s.Set(TodayDayKey, todayDay);
        }
    }

    [OnEvent]
    public void OnNewDay(CycleDayStartedEvent _) => Randomize();

    void GenerateTodayDecal()
    {
        todayDecal = RandomizeDecalId();
        todayDay = DateTime.Now.DayOfYear;
    }

    public void Randomize()
    {
        GenerateTodayDecal();

        foreach (var c in comps)
        {
            c.ShowTexture();
        }
    }

    string RandomizeDecalId() => DecalIds.Count switch
    {
        0 => "",
        1 => DecalIds[0],
        _ => DecalIds[UnityEngine.Random.RandomRangeInt(0, DecalIds.Count)]
    };

    Texture2D GetTextureFrom(string? id) => string.IsNullOrEmpty(id)
        ? Texture2D.whiteTexture
        : decalService.GetDecalTexture(new(id, nameof(DecalTypeEnum.Tails)));

}
