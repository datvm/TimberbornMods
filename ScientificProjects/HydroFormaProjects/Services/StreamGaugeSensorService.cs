namespace HydroFormaProjects.Services;

public class StreamGaugeSensorService(
    IThreadSafeWaterMap waterMap,
    ScienceService sciences,
    IDayNightCycle dayNightCycle,
    MapSize mapSize,
    MapIndexService mapIndexService,
    IThreadSafeColumnTerrainMap columnTerrainMap
) : SimpleProjectListener, ILoadableSingleton
{
    int verticalStride;

    public override string ProjectId { get; } = HydroFormaModUtils.StreamGaugeUpgrade;
    public int MeasureVolumeScienceCost { get; private set; } = 10;

    public override void Load()
    {
        base.Load();
        verticalStride = mapIndexService.VerticalStride;
    }

    public override void OnProjectUnlocked()
    {
        MeasureVolumeScienceCost = (int)ProjectInfo!.Spec.Parameters[0];
    }

    public StreamGaugeSensorMeasurement MeasureSensorLevel(StreamGaugeSensor sensor)
    {
        var coord = sensor.BlockObject.Coordinates;
        var z = coord.z;
        var soilZ = GetSoilLevelAt(coord);


        var level = Mathf.Clamp(waterMap.WaterHeightOrFloor(coord),
            0f, z + sensor.StreamGaugeSpec.MaxWaterLevel);

        return new(level, z, soilZ);
    }

    public bool CanMeasureVolume => sciences.SciencePoints >= MeasureVolumeScienceCost;

    public bool MeasureVolumeRequested(StreamGaugeSensor sensor, bool fullCubeOnly)
    {
        if (sciences.SciencePoints < MeasureVolumeScienceCost) { return false; }
        sciences.SubtractPoints(MeasureVolumeScienceCost);

        sensor.VolumeMeasurement = MeasureVolume(sensor, fullCubeOnly);

        return true;
    }

    StreamGaugeSensorVolumeMeasurement MeasureVolume(StreamGaugeSensor sensor, bool fullCubeOnly)
    {
        const float HeightFluctuationThreshold = 0.1f;
        var volume = 0f;
        float? initialDepth = null;

        var blockCoord = sensor.BlockObject.Coordinates;
        waterMap.TryGetColumnFloor(blockCoord, out var startFloor);

        var startCoord = blockCoord with { z = startFloor };
        HashSet<Vector3Int> visited = [startCoord];
        Stack<Vector3Int> stack = new(visited);

        var neighbors = Deltas.Neighbors6Vector3Int;

        while (stack.Count > 0)
        {
            var coord = stack.Pop();
            
            var depth = Mathf.Clamp01(waterMap.WaterHeightOrFloor(coord) - coord.z);
            initialDepth ??= depth - HeightFluctuationThreshold;

            volume += depth;

            if (depth <= 0
                || (fullCubeOnly && depth < initialDepth)) { continue; }

            foreach (var delta in neighbors)
            {
                var neighbor = coord + delta;
                TryPush(neighbor);
            }

            void TryPush(Vector3Int neighbor)
            {
                if (visited.Contains(neighbor) 
                    || !mapSize.ContainsInTotal(neighbor)) { return; }

                visited.Add(neighbor);
                stack.Push(neighbor);
            }

        }

        return new(volume, visited.Count, dayNightCycle.DayNumber);
    }

    int GetSoilLevelAt(Vector3Int coord)
    {
        var index2D = mapIndexService.CellToIndex(coord.XY());
        var count = columnTerrainMap.GetColumnCount(index2D);

        for (int i = count - 1; i >= 0; i--)
        {
            var ceil = columnTerrainMap.GetColumnCeiling(index2D + i * verticalStride);
            if (ceil <= coord.z)
            {
                return ceil;
            }
        }

        throw new InvalidDataException("No soil underneath, this should not happen");
    }

}

public readonly record struct StreamGaugeSensorMeasurement(float Height, float BaseZ, int SoilZ);
public readonly record struct StreamGaugeSensorVolumeMeasurement(float Volume, int Cubes, int Day);