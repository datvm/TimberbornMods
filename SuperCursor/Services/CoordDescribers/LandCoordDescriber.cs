namespace SuperCursor.Services.CoordDescribers;

[ServicePriority(0)]
public class LandCoordDescriber(
    ISoilContaminationService soilContamination,
    ISoilMoistureService soilMoisture,
    MapIndexService mapIndex,
    IThreadSafeColumnTerrainMap terrain,
    ILoc t
) : ICoordDescriber
{

    public void Describe(StringBuilder builder, in CursorCoordinates block)
    {
        var coord = block.TileCoordinates;

        var index2D = mapIndex.CellToIndex(coord.XY());

        if (!terrain.TryGetIndexAtCeiling(index2D, coord.z, out var index3D)) { return; }

        var moisture = soilMoisture.SoilMoisture(index3D);
        var contamination = soilContamination.Contamination(index3D) * 100;

        builder.AppendLine(t.T("LV.SC.BlockStat", 
            moisture.ToString("F0"),
            contamination.ToString("F0")));
    }

}
