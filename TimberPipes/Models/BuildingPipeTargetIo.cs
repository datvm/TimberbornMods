namespace TimberPipes.Models;

public readonly record struct BuildingTargetPort(Vector3Int Coordinates, Direction3D Direction, PipePortState State);

public readonly record struct LiquidInventory(int Current, int Max)
{
    public int Free => Math.Max(0, Max - Current);
}

public static class BuildingPipeTargetIo
{
    public static bool FaceAllowed(
        bool anyFace,
        IReadOnlyList<BuildingTargetPort> ports,
        PipePortDefinition approach,
        bool give)
    {
        if (anyFace)
        {
            return true;
        }

        var required = give ? PipePortState.OpenIn : PipePortState.OpenOut;
        foreach (var port in ports)
        {
            if (port.Coordinates == approach.Coordinates
                && port.Direction == approach.Direction
                && (port.State & required) != 0)
            {
                return true;
            }
        }

        return false;
    }

    public static List<BuildingTargetPort> TransformPorts(BlockObject blockObject, IEnumerable<PipePortSpec> ports)
    {
        List<BuildingTargetPort> result = [];
        foreach (var portSpec in ports)
        {
            foreach (var d in portSpec.Directions)
            {
                result.Add(new(
                    blockObject.TransformCoordinates(portSpec.Coordinates),
                    blockObject.TransformDirection(d),
                    portSpec.State));
            }
        }

        return result;
    }
}
