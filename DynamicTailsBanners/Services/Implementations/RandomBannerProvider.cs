namespace DynamicTailsBanners.Services.Implementations;

[MultiBind(typeof(IDynamicDecalProvider), AlsoBindSelf = true)]
public class RandomBannerProvider(
    EventBus eb,
    IDayNightCycle dayNightCycle,
    NamedIconProvider namedIconProvider,
    IDecalService decalService
) : IDynamicBannerDecalProvider, ILoadableSingleton
{
    public const string Id = "dynamic-banner-random";
    string IDynamicDecalProvider.Id => Id;

    readonly HashSet<DynamicBuildingDecal> comps = [];

    public void Load()
    {
        eb.Register(this);
    }

    public Texture2D GetTexture(DynamicBuildingDecal comp)
    {
        var opt = comp.Options.GetSettingsOrDefault<RandomBannerProviderOptions>();

        if (opt.CurrentDay != dayNightCycle.DayNumber)
        {
            opt.CurrentDay = dayNightCycle.DayNumber;
            opt.CurrentBannerId = opt.BannerIds.Randomize();
        }

        var id = opt.CurrentBannerId;
        if (id is null) { return namedIconProvider.QuestionMark.texture; }

        return decalService.GetDecalTexture(new(id, nameof(DecalTypeEnum.Banners)));
    }

    public void Register(DynamicBuildingDecal comp)
        => comps.Add(comp);

    public void Unregister(DynamicBuildingDecal comp)
        => comps.Remove(comp);

    public void Rerandomize(DynamicBuildingDecal comp)
    {
        comp.Options.GetSettingsOrDefault<RandomBannerProviderOptions>().CurrentDay = -1;
        comp.ShowTexture();
    }

    [OnEvent]
    public void OnNewDay(CycleDayStartedEvent _)
    {
        foreach (var c in comps)
        {
            c.ShowTexture();
        }
    }

}
