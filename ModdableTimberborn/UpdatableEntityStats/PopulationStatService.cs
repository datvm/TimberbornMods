namespace ModdableTimberborn.UpdatableEntityStats;

public readonly record struct PopulationCounterOptions(PopulationCounterMode Mode, bool CountBeavers, bool CountBots);

public class PopulationStatService(EventBus eb, SamplingPopulationService samplingPopulationService) : ILoadableSingleton, IPostLoadableSingleton
{
    public static readonly ImmutableArray<PopulationCounterMode> PopulationCounterModes
        = TimberUiUtils.GetSortedEnumValues<PopulationCounterMode>();

    public event Action? OnPopulationChanged;

    public void Load()
    {
        eb.Register(this);
    }

    public int GetGlobalData(PopulationCounterOptions opts) => GetData(null, opts);

    public int GetData(DistrictCenter? district, PopulationCounterOptions opts)
    {
        var districtData = district ? samplingPopulationService.GetDistrictData(district) : null;
        districtData ??= samplingPopulationService.GlobalPopulationData;

        return districtData.GetData(opts.Mode, opts.CountBeavers, opts.CountBots);
    }

    [OnEvent]
    public void OnChanged(PopulationChangedEvent _) => OnPopulationChanged?.Invoke();

    public void PostLoad() => OnPopulationChanged?.Invoke();
}
