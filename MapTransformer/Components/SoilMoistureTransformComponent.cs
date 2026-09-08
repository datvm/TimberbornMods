namespace MapTransformer.Components;

[MultiBind(typeof(IMapResizeComponent), Contexts = BindAttributeContext.NonMenu)]
class SoilMoistureTransformComponent(
    SoilMoistureSimulator soilMoistureSimulator,
    IThreadSafeColumnTerrainMap threadSafeColumnTerrainMap,
    MapIndexService mapIndexService,
    TickOnlyArrayService tickOnlyArrayService,
    SoilMoistureSimulationTaskStarter soilMoistureSimulationTaskStarter
) : IMapResizeComponent
{
    public int Order => 50;

    public void Transform(in MapTransformContext ctx)
    {
        var oldVs = soilMoistureSimulator._verticalStride;
        var oldMoisture = soilMoistureSimulator._moistureLevels.GetSpan().ToArray();
        var oldMax = oldVs == 0 ? 1 : Math.Max(1, oldMoisture.Length / oldVs);

        var newVs = mapIndexService.VerticalStride;
        var newMax = Math.Max(1, threadSafeColumnTerrainMap.MaxColumnCount);
        var size = newVs * newMax;

        soilMoistureSimulator._verticalStride = newVs;
        soilMoistureSimulator._moistureLevels = tickOnlyArrayService.Create<float>(size);
        soilMoistureSimulator._lastTickMoistureLevels = tickOnlyArrayService.Create<float>(size);
        soilMoistureSimulator._moistureLevelsChangedLastTick = tickOnlyArrayService.Create<bool>(size);
        soilMoistureSimulator._wateredNeighbours = tickOnlyArrayService.Create<byte>(newVs);
        soilMoistureSimulator._clusterSaturations = tickOnlyArrayService.Create<byte>(newVs);

        MapIndexCopy.CopyColumnFloats(
            oldMoisture,
            oldVs,
            oldMax,
            soilMoistureSimulator._moistureLevels.GetSpan(),
            newVs,
            newMax,
            in ctx);

        soilMoistureSimulationTaskStarter.Load();
    }
}
