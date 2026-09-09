namespace TimberPipes.Models;

public static class BuildingPipeLayout
{
    static readonly Direction3D[] Faces =
    [
        Direction3D.Down,
        Direction3D.Left,
        Direction3D.Up,
        Direction3D.Right,
        Direction3D.Bottom,
        Direction3D.Top,
    ];

    public static BuildingPipeSpec? FromBlockObject(BlockObject blockObject, PipePortState state)
        => FromOccupied(blockObject.Blocks.GetOccupiedCoordinates(), state);

    public static BuildingPipeSpec? FromWaterInterface(BlockObject blockObject, PipePortState state)
    {
        List<Vector3Int> ignored = [];
        if (blockObject.TryGetComponent<WaterInputSpec>(out var input))
        {
            var pipe = input.WaterInputCoordinates;
            ignored.Add(new(pipe.x, pipe.y, blockObject.BaseZ));
        }

        if (blockObject.TryGetComponent<WaterOutputSpec>(out var output))
        {
            ignored.Add(output.WaterCoordinates);
        }

        Vector3Int? entrance = blockObject.Entrance.HasEntrance
            ? blockObject.Entrance.Coordinates
            : null;

        return FromOccupied(
            blockObject.Blocks.GetOccupiedCoordinates(),
            state,
            blockObject.BaseZ,
            entrance,
            ignored);
    }

    public static BuildingPipeSpec? FromOccupied(
        IEnumerable<Vector3Int> occupied,
        PipePortState state,
        int? floorZ = null,
        Vector3Int? entrance = null,
        IEnumerable<Vector3Int>? ignored = null)
    {
        HashSet<Vector3Int> cells = [.. occupied];
        if (cells.Count == 0)
        {
            return null;
        }

        HashSet<Vector3Int> skip = ignored is null ? [] : [.. ignored];
        List<PipePortSpec> ports = [];
        foreach (var cell in cells)
        {
            if (skip.Contains(cell) || (floorZ is { } z && cell.z != z))
            {
                continue;
            }

            var directions = Directions3D.None;
            foreach (var face in Faces)
            {
                var neighbor = cell + face.ToOffset();
                if (cells.Contains(neighbor) || neighbor == entrance)
                {
                    continue;
                }

                directions |= face.ToDirections3D();
            }

            if (directions == Directions3D.None)
            {
                continue;
            }

            ports.Add(new()
            {
                Coordinates = cell,
                Directions = directions,
                State = state,
            });
        }

        if (ports.Count == 0)
        {
            return null;
        }

        ports.Sort(ComparePort);
        return new() { Ports = [.. ports] };
    }

    static int ComparePort(PipePortSpec a, PipePortSpec b)
    {
        var c = a.Coordinates.z.CompareTo(b.Coordinates.z);
        if (c != 0)
        {
            return c;
        }

        c = a.Coordinates.y.CompareTo(b.Coordinates.y);
        return c != 0 ? c : a.Coordinates.x.CompareTo(b.Coordinates.x);
    }
}
