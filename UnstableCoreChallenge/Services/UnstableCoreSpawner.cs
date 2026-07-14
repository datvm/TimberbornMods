namespace UnstableCoreChallenge.Services;

[BindSingleton]
public class UnstableCoreSpawner(
    GameStatService gameStats,
    DefaultEntityTracker<UnstableCoreStabilizer> tracker,
    UnstableCoreStabilizerService coreService,
    BlockObjectSpawningHelper spawningHelper,
    TemplateNameMapper templateNameMapper,
    BlockObjectPlacerService placerService
) : ITickableSingleton, ILoadableSingleton
{
    const float SpawnChancePerHour = .1f;

    int prevHours = -1;
    BlockObjectSpec unstableCoreSpec = null!;
    IBlockObjectPlacer placer = null!;

    public void Load()
    {
        unstableCoreSpec = templateNameMapper.GetTemplate("UnstableCore").GetSpec<BlockObjectSpec>();
        placer = placerService.GetMatchingPlacer(unstableCoreSpec);
    }

    void SpawnCore(Placement placement, UnstableCoreStabilizerInitializer initializer)
    {
        var builder = new EntitySetup.Builder(unstableCoreSpec.Blueprint);
        builder.AddInitComponent(initializer);
        placer.Place(builder, placement);
    }

    internal void TrySpawningCore()
    {
        var spec = coreService.GetCurrentSpec();
        if (tracker.Entities.Count >= spec.MaxBombs) { return; }

        if (Random.Range(0, 1f) > SpawnChancePerHour) { return; }

        if (!spawningHelper.TryFindPlacement(unstableCoreSpec, out var placement, new(WithinBuildingRadius: 20)))
        {
            Debug.LogWarning("Failed to find placement for an unstable core");
            return;
        }

        SpawnCore(placement.Value, coreService.GetChallenge(spec));
    }

    public void Tick()
    {
        var hours = (int)gameStats.GetStat<float>(GameStats.TimeTodayHours);
        if (hours == prevHours) { return; }

        prevHours = hours;
        TrySpawningCore();
    }

}
