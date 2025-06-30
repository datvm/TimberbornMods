namespace HydroFormaProjects.Services;

public class StreamGaugeSensorService(
    ScientificProjectService projects,
    IThreadSafeWaterMap waterMap,
    ScienceService sciences,
    IDayNightCycle dayNightCycle,
    MapSize mapSize
) : BaseProjectService(projects), ILoadableSingleton
{
    readonly ScientificProjectService projects = projects;

    protected override string ProjectId { get; } = HydroFormaModUtils.StreamGaugeUpgrade;
    public int MeasureVolumeScienceCost { get; private set; } = 10;

    public void Load()
    {
        var projectSpec = projects.GetProjectSpec(ProjectId);
        MeasureVolumeScienceCost = (int)projectSpec.Parameters[0];
    }

    public StreamGaugeSensorMeasurement MeasureSensorLevel(StreamGaugeSensor sensor)
    {
        var coord = sensor.BlockObject.Coordinates;
        var z = coord.z;

        var level = Mathf.Clamp(waterMap.WaterHeightOrFloor(coord),
            0f, z + sensor.StreamGaugeSpec.MaxWaterLevel);

        return new(level, z);
    }

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

        var startCoord = sensor.BlockObject.Coordinates;
        HashSet<Vector3Int> visited = [startCoord];
        Stack<Vector3Int> stack = new(visited);

        var neighbors = Deltas.Neighbors6Vector3Int;
        var size = mapSize.TotalSize;

        while (stack.Count > 0)
        {
            var coord = stack.Pop();
            
            var depth = Mathf.Clamp01(waterMap.WaterHeightOrFloor(coord));
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

        ScientificProjectsUtils.Log(() => "StreamGaugeSensor: measured in these cells: " + string.Join(", ", visited));

        return new(volume, visited.Count, dayNightCycle.DayNumber);
    }

}

public readonly record struct StreamGaugeSensorMeasurement(float Height, float BaseZ);
public readonly record struct StreamGaugeSensorVolumeMeasurement(float Volume, int Cubes, int Day);