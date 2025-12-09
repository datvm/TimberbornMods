global using Timberborn.TerrainSystem;
global using Timberborn.WaterSystem;

namespace StreamGaugeVolume;

public class StreamGaugeVolumeService(IThreadSafeWaterMap map, ITerrainService terrain)
{

    static readonly Vector3Int Down = new(0, 0, -1);
    static readonly Vector3Int Up = new(0, 0, 1);
    static readonly Vector3Int[] Directions = [
        new(1, 0, 0),
        new(-1, 0, 0),
        new(0, 1, 0),
        new(0, -1, 0),
        Up,
        Down,
    ];

    public StreamGaugeVolume CalculateVolume(Vector3Int start)
    {
        var total = 0f;
        float? startingDepth = null;

        Queue<Vector3Int> queue = [];
        HashSet<Vector3Int> visited = [];
        HashSet<Vector3Int> counted = [];
        int waterBlockCounter = 0;

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!terrain.Contains(current.XY())) { continue; }

            // Count the current cell
            var hasWater = false;
            var currentDepth = 0f;
            if (!counted.Contains(current))
            {
                if (!map.TryGetColumnFloor(current, out var floor)) { continue; }

                var depth = map.WaterDepth(current);
                if (depth == 0) { continue; }

                for (var i = 0; i < depth; i++)
                {
                    Vector3Int cell = new(current.x, current.y, i + floor);
                    var cellDepth = MathF.Min(1, depth - i);
                    total += cellDepth;
                    counted.Add(cell);

                    if (cellDepth > 0)
                    {
                        waterBlockCounter++;
                    }

                    if (cell == current)
                    {
                        startingDepth ??= cellDepth;
                        currentDepth = cellDepth;
                        hasWater = true;
                    }
                }
            }

            // Add neighbors
            if (hasWater && MathF.Abs(currentDepth - startingDepth!.Value) < .1f) // startingDepth should always have a value here
            {
                foreach (var dir in Directions)
                {
                    var next = current + dir;
                    if (visited.Contains(next)) { continue; }
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        return new(total, waterBlockCounter);
    }

}

public readonly struct StreamGaugeVolume(float volume, int cells)
{
    public float Volume => volume;
    public int Cells => cells;
}
