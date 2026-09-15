namespace ConveyorBelt.Components;

public static class BeltPortIo
{
    public static BeltPortKind RequiredNeighborKind(BeltPortKind kind) => kind switch
    {
        BeltPortKind.In => BeltPortKind.Out,
        BeltPortKind.Out => BeltPortKind.In,
        BeltPortKind.Both => BeltPortKind.Both,
        _ => BeltPortKind.None,
    };

    public static bool HasFacingPort(ImmutableArray<BeltPort> ports, Vector3Int from, BeltPortKind kind)
    {
        foreach (var port in ports)
        {
            if (!port.Faces(from))
            {
                continue;
            }

            if (kind == BeltPortKind.Both ? port.Kind != BeltPortKind.None : port.Allows(kind))
            {
                return true;
            }
        }

        return false;
    }
}
