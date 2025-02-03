global using Timberborn.WaterSystem;
global using Timberborn.TerrainSystem;

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

        Queue<Vector3Int> queue = [];
        HashSet<Vector3Int> visited = [];
        HashSet<Vector3Int> counted = [];

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!terrain.Contains(current.XY())) { continue; }

            // Count the current cell
            var hasWater = false;
            if (!counted.Contains(current))
            {
                if (!map.TryGetColumnFloor(current, out var floor)) { continue; }
                                
                var depth = map.WaterDepth(current);
                if (depth == 0) { continue; }
                
                for (var i = 0; i < depth; i++)
                {
                    Vector3Int cell = new(current.x, current.y, i + floor);
                    if (terrain.Contains(cell.XY()))
                    {
                        total += MathF.Min(1, depth - i);
                        counted.Add(cell);
                    }

                    if (cell == current) { hasWater = true; }
                }
            }

            // Add neighbors
            if (hasWater)
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

        return new(total, counted.Count);
    }

}

public readonly struct StreamGaugeVolume(float volume, int cells)
{
    public float Volume => volume;
    public int Cells => cells;
}
