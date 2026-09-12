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

    public static BuildingPipeSpec? FromOccupied(IEnumerable<Vector3Int> occupied, PipePortState state)
    {
        HashSet<Vector3Int> cells = [.. occupied];
        if (cells.Count == 0)
        {
            return null;
        }

        List<PipePortSpec> ports = [];
        foreach (var cell in cells)
        {
            var directions = Directions3D.None;
            foreach (var face in Faces)
            {
                var neighbor = cell + face.ToOffset();
                if (cells.Contains(neighbor))
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
