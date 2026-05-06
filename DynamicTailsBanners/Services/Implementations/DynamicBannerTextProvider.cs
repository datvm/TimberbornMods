namespace DynamicTailsBanners.Services.Implementations;

[MultiBind(typeof(IDynamicDecalProvider), AlsoBindSelf = true)]
public class DynamicBannerTextProvider(UpdatableEntityStatService statService) : IDynamicBannerDecalProvider, IConnectedDynamicDecal
{
    public const string Id = "dynamic-banner-text";

    string IDynamicDecalProvider.Id => Id;
    public int ExpectedConnectionCount => 1;

    public Texture2D GetTexture(DynamicBuildingDecal comp)
    {
        comp.RendererMaterial.color = GetSettings(comp.Options).Color;
        return GetRenderer(comp).Texture!;
    }

    public void SetStat(DynamicDecalOption opts, IUpdatableEntityStat? stat)
    {
        UnregisterTracker(opts);

        var s = GetSettings(opts);
        s.StatId = stat?.Id;

        if (!s.IsCustomText)
        {
            s.Content = "?";
        }

        RegisterTracker(opts, stat);
    }

    public void SetEntity(DynamicDecalOption opts, EntityComponent? entity) =>
        ChangeSettings(opts, () =>
        {
            opts.Components[0] = entity;
        });

    public void SetCustomText(DynamicDecalOption opts, string text)
    {
        var s = GetSettings(opts);
        if (!string.IsNullOrEmpty(s.StatId)) // Not custom text, it belongs to a stat
        {
            throw new InvalidOperationException("Cannot set custom text for a stat-based banner. StatId must be null to allow custom text.");
        }

        s.Content = text;
        RenderText(text, opts);
    }

    public void SetTextSize(DynamicDecalOption opts, int size)
    {
        var s = GetSettings(opts);
        s.FontSize = size;

        GetRenderer(opts).SetFontSize(size);
    }

    public void ChangeSettings(DynamicDecalOption opts, Action changeSettings)
    {
        UnregisterTracker(opts);
        changeSettings();
        RegisterTracker(opts);
    }

    void RegisterTracker(DynamicDecalOption opts, IUpdatableEntityStat? stat = null)
    {
        try
        {
            UnregisterTracker(opts);

            var s = GetSettings(opts);
            if (stat is null)
            {
                var statId = s.StatId;

                if (string.IsNullOrEmpty(statId)
                    || !statService.TryGetStat(statId, out stat))
                {
                    return;
                }
            }

            if (stat is PopulationStat popStat) // Special case for population stat
            {
                RegisterPopulationStat(opts, popStat);
                return;
            }

            var target = opts.Components[0]?.GetComponent<UpdatableEntityStatComponent>();
            if (!stat.CanTrack(target)
                || !stat.TryGetTracker(target, out var tracker))
            {
                return;
            }

            SetupTracker(tracker, opts);
        }
        finally
        {
            RenderText(opts);
        }
    }

    void SetupTracker(IStatTracker tracker, DynamicDecalOption opts)
    {
        if (tracker is IEntityStatTracker et)
        {
            et.OnEntityLost += (_, _) => OnEntityLost(opts);
        }

        tracker.OnValueChanged += (_, _) => OnTextChanged(opts, tracker);
        tracker.Start();

        opts.SetReference(tracker);
        tracker.ForceUpdating();
    }

    void OnEntityLost(DynamicDecalOption opts) => SetEntity(opts, null);
    void OnTextChanged(DynamicDecalOption opts, IStatTracker tracker)
    {
        if (!opts)
        {
            Debug.LogWarning($"DynamicDecalOption is null. This should not be happening.");
            return;
        }

        var s = GetSettings(opts);
        var c = s.Content = tracker.ValueFormatted;

        RenderText(c, opts);
    }

    static void RenderText(DynamicDecalOption opts)
    {
        var s = GetSettings(opts);
        RenderText(s.Content, opts);
    }

    static void RenderText(string c, DynamicDecalOption opts)
    {
        GetRenderer(opts).SetContent(c);
        opts.RefreshDecalTexture();
    }

    async void RegisterPopulationStat(DynamicDecalOption opts, PopulationStat popStat)
    {
        var dc = opts.Components[0]?.GetComponent<DistrictCenter>();
        var updatable = dc ? dc!.GetUpdatableStatComponent() : null;

        var s = GetSettings(opts);
        PopulationCounterOptions popOpts = s.GetPopulationOptions();

        IStatTracker tracker;
        if (updatable)
        {
            if (!popStat.TryGetTracker(popOpts, updatable!, out var popTracker))
            {
                return;
            }

            tracker = popTracker;
        }
        else
        {
            tracker = popStat.GetGlobalTracker(popOpts);
        }

        SetupTracker(tracker, opts);

    }

    void UnregisterTracker(DynamicDecalOption opts)
    {
        var tracker = GetTracker(opts);

        tracker?.Dispose();
        opts.ClearReference();
    }

    public void Register(DynamicBuildingDecal comp)
    {
        var opts = comp.Options;
        GetSettings(opts); // Also use for setting default

        GetRenderer(comp).Enable();

        RegisterTracker(opts);
    }

    public void Unregister(DynamicBuildingDecal comp)
    {
        UnregisterTracker(comp.Options);
        GetRenderer(comp).Disable();
        comp.RendererMaterial.color = Color.white;
    }

    static DynamicBannerTextRenderer GetRenderer(BaseComponent comp) => comp.GetComponent<DynamicBannerTextRenderer>();

    static DynamicBannerTextOptions GetSettings(DynamicDecalOption opts)
        => opts.GetSettingsOrDefault<DynamicBannerTextOptions>();

    static IStatTracker? GetTracker(DynamicDecalOption opts) => opts.GetReference<IStatTracker>();

}
