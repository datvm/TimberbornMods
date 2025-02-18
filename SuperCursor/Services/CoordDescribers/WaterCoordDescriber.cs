namespace SuperCursor.Services.CoordDescribers;

public class WaterCoordDescriber(IThreadSafeWaterMap water, ILoc loc) : ICoordDescriber
{

    public void Describe(StringBuilder builder, in CursorCoordinates block)
    {
        var coord = block.TileCoordinates;
        var depth = water.WaterDepth(coord);
        if (depth <= 0) { return; }

        var depthFloor = water.WaterHeightOrFloor(coord) - coord.z;
        builder.AppendLine(loc.T("LV.SC.Water").Bigger().Bold());

        if (Mathf.Approximately(depthFloor, depth))
        {
            builder.AppendLine(loc.T("LV.SC.WaterDepth", depth.ToString("F2")));
        }
        else
        {
            builder.AppendLine(loc.T("LV.SC.WaterDepthFrom", depth.ToString("F2"), depthFloor.ToString("F2")));
        }

        var flowDirection = water.WaterFlowDirection(coord);
        var current = MathF.Max(Mathf.Abs(flowDirection.x), Mathf.Abs(flowDirection.y));
        builder.AppendLine(loc.T("LV.SC.WaterCurrent", current.ToString("F2")));

        var conta = water.ColumnContamination(coord) * 100;
        builder.AppendLine(loc.T("LV.SC.WaterContamination", conta.ToString("F0")));
    }

}
