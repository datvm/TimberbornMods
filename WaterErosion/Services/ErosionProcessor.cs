namespace WaterErosion.Services;

[BindSingleton]
public class ErosionProcessor(
    MSettings s,
    IDayNightCycle dayNight,
    ErosionMap erosionMap,
    MapIndexService mapIndexService,
    TerrainMap terrainMap,
    IThreadSafeWaterMap waterMap,
    BlockageService blockageService
) : ITickableSingleton, ILoadableSingleton
{
    static readonly Vector3Int[] Neighbors = Deltas.Neighbors6Vector3Int;

    const int MaxChunk = 10;
    int processingPerChunk = 100 * 100; // 256x256 map would need ~6.5 = 7 chunks

    float erosionRate, badwaterErosionRate;
    float[] blockageThreshold = new float[3];

    Index2DEnumerator enumerator;
    int currChunkIndex = -1;
    long currTick = 0;
    long[] lastChunkTick = [];

    readonly Dictionary<Vector3Int, float> changed = [];

    public void Load()
    {
        var ticksInADay = dayNight.HoursToTicks(1) * 24;

        erosionRate = s.ErosionRate.Value / 100f / ticksInADay;
        badwaterErosionRate = s.BadwaterErosionRate.Value / 100f / ticksInADay;

        blockageThreshold = new float[BlockageService.BlockageCount];

        var fullThr = blockageThreshold[0] = s.BlockageThreshold.Value / 100f; // >75% = stay dirt
        var thrPerReduction = fullThr / (BlockageService.BlockageCount - 1); // 75% - >50% = convert to first blockage
        for (int i = 1; i < BlockageService.BlockageCount; i++)
        {
            blockageThreshold[i] = blockageThreshold[i - 1] - thrPerReduction;
        }

        enumerator = mapIndexService.Indices2D;

        var mapSize = mapIndexService.TotalSize;
        int sx = mapSize.x, sy = mapSize.y;
        var cellCount = (sx + 2) * (sy + 2); // +2 for padding

        var chunkCount = Math.Min(MaxChunk, Mathf.CeilToInt((float)cellCount / processingPerChunk));
        processingPerChunk = Mathf.CeilToInt((float)cellCount / chunkCount); // Make sure we cover the whole map in the given number of chunks

        lastChunkTick = new long[chunkCount + 1]; // +1 for the final chunk that is skipped
    }

    public void Tick()
    {
        var counter = 0;
        currChunkIndex++;
        currTick++;

        var tickPassed = currTick - lastChunkTick[currChunkIndex];
        lastChunkTick[currChunkIndex] = currTick;

        var waterColumns = waterMap.WaterColumns;
        var flowDirection = waterMap.FlowDirections;

        var map = erosionMap.Map;
        var max = map.Length;

        while (enumerator.MoveNext())
        {
            var index2D = enumerator.Current;
            var x = enumerator._currentX;
            var y = enumerator._currentY;

            var columnCount = waterMap.ColumnCount(index2D);
            for (int i = 0; i < columnCount; i++)
            {
                var coords = new Vector3Int(x, y, i);

                var index3D = mapIndexService.CoordinatesToIndex3D(coords);
                var column = waterColumns[index3D];
                var depth = column.WaterDepth;
                if (depth == 0) { continue; }

                var flow = flowDirection[index3D].magnitude;
                if (flow <= .001f) { continue; }

                var rate = Mathf.Lerp(erosionRate, badwaterErosionRate, column.Contamination);
                var change = flow * rate * tickPassed;
                var floor = column.Floor;
                for (int z = 0; z < depth; z++)
                {
                    var waterCoords = new Vector3Int(x, y, floor + z);

                    foreach (var n in Neighbors)
                    {
                        var zCoords = waterCoords + n;
                        var z3D = mapIndexService.CoordinatesToIndex3D(zCoords);

                        if (z3D >= max) { continue; }

                        var value = map[z3D] - change;
                        map[z3D] = value;
                        changed[zCoords] = value;
                    }
                }
            }

            counter++;
            if (counter >= processingPerChunk) { break; }
        }

        if (counter == 0) // Finished a full pass, reset enumerator for next pass
        {
            currChunkIndex = -1;
            enumerator = mapIndexService.Indices2D;
            currTick--;

            Tick();
            return;
        }

        ProcessChanges();
    }

    void ProcessChanges()
    {
        if (changed.Count == 0) { return; }
        foreach (var (c, erosion) in changed)
        {
            if (terrainMap.IsTerrainVoxel(c))
            {
                if (erosion <= blockageThreshold[0])
                {
                    blockageService.ErodeDirtAt(c);
                }
            }
            else if (blockageService.TryGetBlockageAt(c, out var blockage))
            {
                if (erosion <= 0)
                {
                    blockageService.ErodeBlockage(blockage, 0f);
                }
                else if (blockage.Order < blockageThreshold.Length && erosion <= blockageThreshold[blockage.Order])
                {
                    blockageService.ErodeBlockage(blockage, erosion);
                }
            }
        }

        changed.Clear();
    }



}
