namespace BuildingRenovations.Services;

[BindSingleton]
public class BuildingRenovationService(
    ILoc t,
    IDayNightCycle dayNightCycle,
    RenovationRegistry registry,
    DistroService distroService,
    RecoveredGoodStackSpawner goodStackSpawner
)
{
    public readonly ILoc t = t;
    public readonly IDayNightCycle dayNightCycle = dayNightCycle;
    public readonly RenovationRegistry registry = registry;
    public readonly DistroService distroService = distroService;
    public readonly RecoveredGoodStackSpawner goodStackSpawner = goodStackSpawner;

    public float PartialDayNumber => dayNightCycle.PartialDayNumber;

    public string? GetRequiredRenovationIdReason(BuildingRenovationComponent building, string? renoId) 
        => renoId is null || building.HasActive(renoId) 
        ? null
        : t.T("LV.BRe.RenovationRequired", registry.Get(renoId).Name);

}
