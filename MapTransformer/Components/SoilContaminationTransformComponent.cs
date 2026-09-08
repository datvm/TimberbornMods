namespace MapTransformer.Components;

[MultiBind(typeof(IMapResizeComponent), Contexts = BindAttributeContext.NonMenu)]
class SoilContaminationTransformComponent(
    SoilContaminationSimulator soilContaminationSimulator,
    IThreadSafeColumnTerrainMap threadSafeColumnTerrainMap,
    MapIndexService mapIndexService,
    TickOnlyArrayService tickOnlyArrayService,
    SoilContaminationSimulationTaskStarter soilContaminationSimulationTaskStarter
) : IMapResizeComponent
{
    public int Order => 60;

    public void Transform(in MapTransformContext ctx)
    {
        var oldVs = soilContaminationSimulator._verticalStride;
        var oldLevels = soilContaminationSimulator._contaminationLevels.GetSpan().ToArray();
        var oldCandidates = soilContaminationSimulator._contaminationCandidates.GetSpan().ToArray();
        var oldMax = oldVs == 0 ? 1 : Math.Max(1, oldLevels.Length / oldVs);

        var newVs = mapIndexService.VerticalStride;
        var newMax = Math.Max(1, threadSafeColumnTerrainMap.MaxColumnCount);
        var size = newVs * newMax;

        soilContaminationSimulator._verticalStride = newVs;
        soilContaminationSimulator._contaminationLevels = tickOnlyArrayService.Create<float>(size);
        soilContaminationSimulator._contaminationCandidates = tickOnlyArrayService.Create<float>(size);
        soilContaminationSimulator._lastTickContaminationCandidates = tickOnlyArrayService.Create<float>(size);
        soilContaminationSimulator._contaminationsChangedLastTick = tickOnlyArrayService.Create<bool>(size);

        MapIndexCopy.CopyColumnFloats(
            oldLevels,
            oldVs,
            oldMax,
            soilContaminationSimulator._contaminationLevels.GetSpan(),
            newVs,
            newMax,
            in ctx);
        MapIndexCopy.CopyColumnFloats(
            oldCandidates,
            oldVs,
            oldMax,
            soilContaminationSimulator._contaminationCandidates.GetSpan(),
            newVs,
            newMax,
            in ctx);

        soilContaminationSimulationTaskStarter.Load();
    }
}
