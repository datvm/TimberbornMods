namespace DynamicTailsBanners.Services.Implementations;

[MultiBind(typeof(IDynamicDecalProvider), AlsoBindSelf = true)]
public class DynamicIconBannerProvider(
    NamedIconProvider namedIconProvider,
    UpdatableEntityStatService statService
) : IDynamicBannerDecalProvider, IConnectedDynamicDecal
{
    public const string Id = "dynamic-banner-building";
    string IDynamicDecalProvider.Id => "dynamic-banner-building";
    public int ExpectedConnectionCount => 1;

    public Texture2D GetTexture(DynamicBuildingDecal comp)
    {
        var opts = comp.Options;

        var s = GetSettings(opts);
        comp.RendererMaterial.color = s.Color;

        var stat = GetTracker(opts);
        return stat?.Value?.texture ?? QuestionMark;
    }

    public void SetEntity(DynamicDecalOption opts, EntityComponent? entity)
    {
        UnregisterTracker(opts);
        opts.Components[0] = entity;
        RegisterTracker(opts, null);
    }

    public void SetStat(DynamicDecalOption opts, IUpdatableEntityStat? stat)
    {
        UnregisterTracker(opts);
        GetSettings(opts).StatId = stat?.Id ?? "";
        RegisterTracker(opts, stat);
    }

    void RegisterTracker(DynamicDecalOption opts, IUpdatableEntityStat? stat)
    {
        try
        {
            UnregisterTracker(opts);

            var target = opts.Components[0];
            if (!target) { return; }

            if (stat is null)
            {
                var s = GetSettings(opts);
                var statId = s.StatId;

                if (string.IsNullOrEmpty(statId)
                    || !statService.TryGetStat(statId, out stat))
                {
                    return;
                }
            }

            var updatable = target!.GetUpdatableStatComponent();
            if (stat is not IImageStat imgStat || !imgStat.CanTrack(updatable)) { return; }

            if (!imgStat.TryGetTracker(updatable, out var tracker)) { return; }

            opts.SetReference(tracker);
            tracker.OnEntityLost += (_, _) => OnEntityLost(opts);            
            tracker.Start();
            tracker.OnValueChanged += (_, _) => opts.RefreshDecalTexture();
        }
        finally
        {
            opts.RefreshDecalTexture();
        }
        
    }

    void OnEntityLost(DynamicDecalOption opts) 
        => SetEntity(opts, null);

    void UnregisterTracker(DynamicDecalOption opts)
    {
        var tracker = GetTracker(opts);
        tracker?.Dispose();
        opts.ClearReference();
    }

    public void Register(DynamicBuildingDecal comp)
    {
        comp.Options.SetDefault<DynamicIconBannerOptions>();
        RegisterTracker(comp.Options, null);
    }

    public void Unregister(DynamicBuildingDecal comp)
    {
        UnregisterTracker(comp.Options);
        comp.RendererMaterial.color = Color.white;
    }

    Texture2D QuestionMark => namedIconProvider.QuestionMark.texture;

    static DynamicIconBannerOptions GetSettings(DynamicDecalOption opts)
        => opts.GetSettingsOrDefault<DynamicIconBannerOptions>();

    static IImageStatTracker? GetTracker(DynamicDecalOption opts)
        => opts.GetReference<IImageStatTracker>();

}
