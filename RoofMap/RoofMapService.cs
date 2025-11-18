namespace RoofMap;

public class RoofMapService(
    TerrainMap terrainMap,
    MapIndexService mapIndexService,
    EventBus eb,
    IThreadSafeColumnTerrainMap columnTerrainMap
) : IPostLoadableSingleton
{

    public ImmutableArray<RoofMapCell> Cells { get; private set; }
    public RoofMapCell GetCellAt(Vector2Int cell) => Cells[mapIndexService.CellToIndex(cell)];
    public RoofMapCell GetCellAt(Vector3Int cell) => Cells[mapIndexService.CellToIndex(new(cell.x, cell.y))];

    public int GetRoofAt(Vector2Int cell, RoofType roofType = RoofType.Any) => GetRoofAt(mapIndexService.CellToIndex(cell), roofType);
    public int GetRoofAt(int index2D, RoofType roofType = RoofType.Any)
    {
        var cell = Cells[index2D];
        int highest = -1;

        if ((roofType & RoofType.Terrain) != 0)
        {
            highest = cell.HighestTerrain;
        }

        if ((roofType & RoofType.BlockObject) != 0)
        {
            if (cell.HighestBlockObjectTop > highest)
            {
                highest = cell.HighestBlockObjectTop.Value;
            }
        }
        else if ((roofType & RoofType.SolidBlockObject) != 0) // No need to consider SolidBlockObject if BlockObject is already considered
        {
            if (cell.HighestSolidBlockObjectTop > highest)
            {
                highest = cell.HighestSolidBlockObjectTop.Value;
            }
        }

        return highest;
    }

    public void PostLoad()
    {
        InitMap();

        terrainMap.TerrainAdded += OnTerrainAdded;
        terrainMap.TerrainRemoved += OnTerrainRemoved;
        eb.Register(this);

        columnTerrainMap.ColumnMovedUp += (_, i) => Debug.Log($"Column at index {i} moved up.");
        columnTerrainMap.ColumnMovedDown += (_, i) => Debug.Log($"Column at index {i} moved down.");
        columnTerrainMap.ColumnReset += (_, i) => Debug.Log($"Column at index {i} was reset.");
        columnTerrainMap.MaxTerrainColumnCountChanged += (_, count) => Debug.Log($"Max terrain column count changed to {count}.");
    }

    void InitMap()
    {
        var cells = new RoofMapCell[mapIndexService.VerticalStride];
        var enumerator = mapIndexService.Indices2D;
        while (enumerator.MoveNext())
        {
            var curr = enumerator.Current;

            var c = cells[curr] = new(enumerator._currentX - 1, enumerator._currentY - 1, curr);
            c.HighestTerrain = FindHighestTerrain(new(c.X, c.Y));
        }

        Cells = [.. cells];
    }

    void OnTerrainRemoved(object sender, Vector3Int e)
    {
        var cell = GetCellAt(e);
        if (e.z >= cell.HighestTerrain)
        {
            cell.HighestTerrain = FindHighestTerrain(new(e.x, e.y));
        }
    }

    void OnTerrainAdded(object sender, Vector3Int e)
    {
        var cell = GetCellAt(e);

        if (e.z > cell.HighestTerrain)
        {
            cell.HighestTerrain = e.z;
        }
    }

    [OnEvent]
    public void OnBlockObjectFinished(EnteredFinishedStateEvent e)
    {

    }

    [OnEvent]
    public void OnBlockObjectExitFinished(ExitedFinishedStateEvent e)
    {

    }

    int FindHighestTerrain(Vector2Int c)
    {
        var index2D = mapIndexService.CellToIndex(c);
        var columns = columnTerrainMap.TerrainColumns;
        var columnCount = columnTerrainMap.GetColumnCount(index2D);

        var height = 0;
        Debug.Log($"Finding highest terrain at {c}:");

        for (int i = 0; i < columnCount; i++)
        {
            var column = columns[mapIndexService.Index2DTo3D(index2D, i)];
            Debug.Log($"- Column {i}: {column.Ceiling}");

            if (column.Ceiling > height)
            {
                height = column.Ceiling;
            }
        }

        return height;
    }

}

[Flags]
public enum RoofType
{
    None = 0,
    Terrain = 1,
    BlockObject = 2,
    SolidBlockObject = 4,
    Any = Terrain | BlockObject | SolidBlockObject
}