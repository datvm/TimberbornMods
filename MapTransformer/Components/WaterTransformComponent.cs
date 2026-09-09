namespace MapTransformer.Components;

[MultiBind(typeof(IMapResizeComponent), Contexts = BindAttributeContext.NonMenu)]
class WaterTransformComponent(
    WaterSimulator waterSimulator,
    ThreadSafeWaterMap threadSafeWaterMap,
    MapIndexService mapIndexService,
    TickOnlyArrayService tickOnlyArrayService,
    WaterSimulationTaskStarter waterSimulationTaskStarter
) : IMapResizeComponent
{
    public int Order => 30;

    public void Transform(in MapTransformContext ctx)
    {
        var oldVs = waterSimulator._verticalStride;
        var oldCounts = waterSimulator._columnCounts.GetSpan().ToArray();
        var oldColumns = waterSimulator._waterColumns.GetSpan().ToArray();

        RebuildColumns();
        CopyDepths(oldCounts, oldColumns, oldVs, in ctx);

        threadSafeWaterMap._verticalStride = mapIndexService.VerticalStride;
        threadSafeWaterMap._threadSafeColumnCounts = new byte[mapIndexService.MaxIndex];
        threadSafeWaterMap._threadSafeWaterColumns = new ReadOnlyWaterColumn[mapIndexService.VerticalStride];
        threadSafeWaterMap._waterFlowDirections = new Vector2[mapIndexService.VerticalStride];
        threadSafeWaterMap.MaxColumnCount = 0;
        threadSafeWaterMap.Update();

        waterSimulationTaskStarter.Load();
    }

    void RebuildColumns()
    {
        waterSimulator._mapSize = mapIndexService.TotalSize;
        waterSimulator._maxColumnHeight = waterSimulator._mapSize.z + 1;
        waterSimulator._stride = mapIndexService.Stride;
        waterSimulator._verticalStride = mapIndexService.VerticalStride;
        waterSimulator.MaxColumnCount = 1;

        var vs = waterSimulator._verticalStride;
        var maxIndex = mapIndexService.MaxIndex;
        waterSimulator._outflows = tickOnlyArrayService.Create<ColumnOutflows>(vs);
        waterSimulator._horizontalObstacles = tickOnlyArrayService.Create<byte>(vs * waterSimulator._maxColumnHeight);
        waterSimulator._baseLevelFlows = tickOnlyArrayService.Create<WaterFlow>(maxIndex);
        waterSimulator._baseLevelDiffusions = tickOnlyArrayService.Create<Diffusions>(maxIndex);
        waterSimulator._directedFlows = tickOnlyArrayService.Create<List<DirectedFlow>>(maxIndex);
        waterSimulator._targetedDiffusions = tickOnlyArrayService.Create<List<TargetedDiffusion>>(maxIndex);
        var directed = waterSimulator._directedFlows.GetSpan();
        var targeted = waterSimulator._targetedDiffusions.GetSpan();
        for (var i = 0; i < maxIndex; i++)
        {
            directed[i] = new List<DirectedFlow>(0);
            targeted[i] = new List<TargetedDiffusion>(0);
        }

        waterSimulator._contaminationsBuffer = tickOnlyArrayService.Create<float>(vs);
        waterSimulator._targetedDiffusionCount = tickOnlyArrayService.Create<byte>(vs);
        waterSimulator.CreateColumns();
    }

    void CopyDepths(byte[] oldCounts, WaterColumn[] oldColumns, int oldVs, in MapTransformContext ctx)
    {
        var t = ctx.Transform;
        var newCounts = waterSimulator._columnCounts.GetSpan();
        var newColumns = waterSimulator._waterColumns.GetSpan();
        var newVs = waterSimulator._verticalStride;
        var size = t.NewTerrainSize;
        for (var y = 0; y < size.y; y++)
        {
            for (var x = 0; x < size.x; x++)
            {
                if (!t.TryUnmapColumn(new Vector2Int(x, y), out var src))
                {
                    continue;
                }

                var oldI = ctx.OldMapIndex.CellToIndex(src);
                var newI = ctx.MapIndex.CellToIndex(new Vector2Int(x, y));
                if (oldI >= oldCounts.Length || newI >= newCounts.Length)
                {
                    continue;
                }

                var oldN = oldCounts[oldI];
                var newN = newCounts[newI];
                for (var ni = 0; ni < newN; ni++)
                {
                    ref var destCol = ref newColumns[ni * newVs + newI];
                    for (var oi = 0; oi < oldN; oi++)
                    {
                        var srcCol = oldColumns[oi * oldVs + oldI];
                        if (!t.TryMapHeight(srcCol.Floor, out var mappedFloor) || mappedFloor != destCol.Floor)
                        {
                            continue;
                        }

                        destCol.WaterDepth = srcCol.WaterDepth;
                        destCol.OldWaterDepth = srcCol.OldWaterDepth;
                        destCol.Contamination = srcCol.Contamination;
                        destCol.Overflow = srcCol.Overflow;
                        break;
                    }
                }
            }
        }

        waterSimulator.AnyColumnChanged = true;
    }
}
