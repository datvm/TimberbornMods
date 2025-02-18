namespace SuperCursor.Services.CoordDescribers;

[ServicePriority(0)]
public class CoordNumberDescriber(ILoc loc) : ICoordDescriber
{
    public void Describe(StringBuilder builder, in CursorCoordinates block)
    {
        var coord = block.TileCoordinates;

        builder.AppendLine(loc.T("LV.SC.Coord", 
            coord.z, 
            coord.x.ToString("D3"), 
            coord.y.ToString("D3")
        ));
    }
}
