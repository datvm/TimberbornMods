namespace MapTransformer.Tests;

public class MapTransformTests
{
    static readonly Vector3Int Terrain = new(4, 6, 8);
    static readonly Vector3Int Total = new(4, 6, 18);

    [Fact]
    public void ResizeMinMinKeepsOrigin()
    {
        var t = MapTransform.ResizeXy(Terrain, Total, new(6, 8), Vector2Int.zero, EnlargeFill.Empty);

        Assert.Equal(8, t.NewTerrainSize.z);
        Assert.Equal(18, t.NewTotalSize.z);
        Assert.True(t.TryMapCell(new(0, 0, 3), out var dest));
        Assert.Equal(new Vector3Int(0, 0, 3), dest);
        Assert.True(t.TryUnmapCell(dest, out var src));
        Assert.Equal(new Vector3Int(0, 0, 3), src);
        Assert.False(t.TryUnmapCell(new(5, 0, 3), out _));
    }

    [Fact]
    public void ResizeMaxMaxShiftsOrigin()
    {
        var t = MapTransform.ResizeXy(Terrain, Total, new(6, 8), new Vector2Int(4, 6), EnlargeFill.Empty);

        Assert.Equal(new Vector2Int(2, 2), t.PivotOffset);
        Assert.True(t.TryMapCell(new(0, 0, 1), out var dest));
        Assert.Equal(new Vector3Int(2, 2, 1), dest);
    }

    [Fact]
    public void ResizeShrinkMaxKeepsFarCorner()
    {
        var oldTerrain = new Vector3Int(50, 50, 8);
        var oldTotal = new Vector3Int(50, 50, 18);
        var t = MapTransform.ResizeXy(oldTerrain, oldTotal, new(47, 47), new Vector2Int(50, 50), EnlargeFill.Empty);

        Assert.Equal(new Vector2Int(-3, -3), t.PivotOffset);
        Assert.False(t.TryMapCell(new(0, 0, 1), out _));
        Assert.False(t.TryMapCell(new(2, 2, 1), out _));
        Assert.True(t.TryMapCell(new(3, 3, 1), out var dest));
        Assert.Equal(new Vector3Int(0, 0, 1), dest);
        Assert.True(t.TryMapCell(new(49, 49, 1), out var far));
        Assert.Equal(new Vector3Int(46, 46, 1), far);
        Assert.True(t.TryUnmapOrFill(new(0, 0, 1), out var src));
        Assert.Equal(new Vector3Int(3, 3, 1), src);
    }

    [Fact]
    public void ResizeHeightAddsEmptyUnmapMiss()
    {
        var t = MapTransform.ResizeHeight(Terrain, Total, 12, 22);

        Assert.Equal(new Vector3Int(4, 6, 12), t.NewTerrainSize);
        Assert.Equal(new Vector3Int(4, 6, 22), t.NewTotalSize);
        Assert.True(t.TryUnmapCell(new(1, 1, 3), out _));
        Assert.False(t.TryUnmapOrFill(new(1, 1, 10), out _));
    }

    [Fact]
    public void ResizeFillCopyEdge()
    {
        var t = MapTransform.ResizeXy(Terrain, Total, new(6, 6), Vector2Int.zero, EnlargeFill.CopyEdge);

        Assert.True(t.TryUnmapOrFill(new(5, 0, 2), out var src));
        Assert.Equal(new Vector3Int(3, 0, 2), src);
    }

    [Fact]
    public void ResizeFillMirror()
    {
        var t = MapTransform.ResizeXy(Terrain, Total, new(8, 6), Vector2Int.zero, EnlargeFill.Mirror);

        Assert.True(t.TryUnmapOrFill(new(4, 1, 2), out var src));
        Assert.Equal(new Vector3Int(3, 1, 2), src);
    }

    [Fact]
    public void ResizeMidPivotScalesWithSize()
    {
        var t = MapTransform.ResizeXy(Terrain, Total, new(8, 12), new Vector2Int(2, 3), EnlargeFill.Empty);

        Assert.Equal(new Vector2Int(2, 3), t.PivotOffset);
        Assert.True(t.TryMapCell(new(0, 0, 1), out var dest));
        Assert.Equal(new Vector3Int(2, 3, 1), dest);
        Assert.True(t.TryMapCell(new(2, 3, 1), out var mid));
        Assert.Equal(new Vector3Int(4, 6, 1), mid);
    }

    [Fact]
    public void Rotate90SwapsSizeAndInverts()
    {
        var t = MapTransform.RotateCw(Terrain, Total, 1);

        Assert.Equal(new Vector3Int(6, 4, 8), t.NewTerrainSize);
        Assert.True(t.TryMapCell(new(1, 2, 3), out var dest));
        Assert.Equal(new Vector3Int(2, 2, 3), dest);
        Assert.True(t.TryUnmapCell(dest, out var src));
        Assert.Equal(new Vector3Int(1, 2, 3), src);
    }

    [Fact]
    public void FlipXMirrorsWidth()
    {
        var t = MapTransform.Flip(Terrain, Total, flipX: true, flipY: false);

        Assert.True(t.TryMapCell(new(0, 2, 1), out var dest));
        Assert.Equal(new Vector3Int(3, 2, 1), dest);
        Assert.True(t.TryUnmapCell(dest, out var src));
        Assert.Equal(new Vector3Int(0, 2, 1), src);
    }

    [Fact]
    public void AddHeightShiftsZ()
    {
        var t = MapTransform.AddHeight(Terrain, Total, 2, increaseTerrainHeight: true);

        Assert.Equal(10, t.NewTerrainSize.z);
        Assert.Equal(20, t.NewTotalSize.z);
        Assert.True(t.TryMapCell(new(1, 1, 0), out var dest));
        Assert.Equal(new Vector3Int(1, 1, 2), dest);
        Assert.False(t.TryUnmapCell(new(1, 1, 1), out _));
        Assert.True(t.TryUnmapCell(new(1, 1, 2), out var src));
        Assert.Equal(new Vector3Int(1, 1, 0), src);
        Assert.False(t.TryUnmapOrFill(new(1, 1, 0), out _));
        Assert.True(t.TryUnmapColumn(new(1, 1), out var col));
        Assert.Equal(new Vector2Int(1, 1), col);
    }

    [Fact]
    public void AddHeightWithoutIncreaseClipsTop()
    {
        var t = MapTransform.AddHeight(Terrain, Total, 2, increaseTerrainHeight: false);

        Assert.Equal(8, t.NewTerrainSize.z);
        Assert.Equal(18, t.NewTotalSize.z);
        Assert.True(t.TryMapCell(new(1, 1, 0), out var dest));
        Assert.Equal(new Vector3Int(1, 1, 2), dest);
        Assert.True(t.TryMapCell(new(1, 1, 15), out var dest2));
        Assert.Equal(new Vector3Int(1, 1, 17), dest2);
        Assert.False(t.TryMapCell(new(1, 1, 16), out _));
        Assert.False(t.TryUnmapCell(new(1, 1, 1), out _));
        Assert.True(t.TryUnmapColumn(new(1, 1), out var col));
        Assert.Equal(new Vector2Int(1, 1), col);
    }

    [Fact]
    public void RotateArea90()
    {
        var t = MapTransform.RotateCw(Terrain, Total, 1, transformArea: true, new Vector2Int(1, 1), 2);

        Assert.Equal(Terrain, t.NewTerrainSize);
        Assert.Null(t.ErrorLocKey());
        Assert.True(t.TryMapCell(new(1, 1, 3), out var a));
        Assert.Equal(new Vector3Int(1, 2, 3), a);
        Assert.True(t.TryUnmapCell(a, out var src));
        Assert.Equal(new Vector3Int(1, 1, 3), src);
        Assert.True(t.TryMapCell(new(0, 0, 3), out var outside));
        Assert.Equal(new Vector3Int(0, 0, 3), outside);
    }

    [Fact]
    public void FlipAreaX()
    {
        var t = MapTransform.Flip(Terrain, Total, flipX: true, flipY: false, transformArea: true, new Vector2Int(1, 1), 2);

        Assert.Equal(Terrain, t.NewTerrainSize);
        Assert.True(t.TryMapCell(new(1, 1, 0), out var a));
        Assert.Equal(new Vector3Int(2, 1, 0), a);
        Assert.True(t.TryUnmapCell(a, out var src));
        Assert.Equal(new Vector3Int(1, 1, 0), src);
        Assert.True(t.TryMapCell(new(0, 1, 0), out var outside));
        Assert.Equal(new Vector3Int(0, 1, 0), outside);
    }

    [Fact]
    public void AreaExceedsMap()
    {
        var t = MapTransform.RotateCw(Terrain, Total, 1, transformArea: true, new Vector2Int(3, 0), 2);

        Assert.Equal("LV.MTr.AreaExceedsMap", t.ErrorLocKey());
    }

    [Fact]
    public void AreaZeroSizeExceeds()
    {
        var t = MapTransform.Flip(Terrain, Total, true, false, transformArea: true, Vector2Int.zero, 0);

        Assert.Equal("LV.MTr.AreaExceedsMap", t.ErrorLocKey());
    }

    [Fact]
    public void MapPlacement1x1Rotate90()
    {
        var t = MapTransform.RotateCw(Terrain, Total, 1);
        var src = new Placement(new Vector3Int(1, 2, 3), Orientation.Cw0, FlipMode.Unflipped);

        Assert.True(t.TryMapPlacement(src, new Vector3Int(1, 1, 1), out var dest));
        Assert.Equal(new Vector3Int(2, 2, 3), dest.Coordinates);
        Assert.Equal(Orientation.Cw90, dest.Orientation);
    }

    [Fact]
    public void MapPlacement2x1Rotate90()
    {
        var t = MapTransform.RotateCw(Terrain, Total, 1);
        var src = new Placement(new Vector3Int(1, 1, 0), Orientation.Cw0, FlipMode.Unflipped);

        Assert.True(t.TryMapPlacement(src, new Vector3Int(2, 1, 1), out var dest));
        Assert.Equal(Orientation.Cw90, dest.Orientation);
        Assert.True(t.TryMapCell(new(1, 1, 0), out var a));
        Assert.True(t.TryMapCell(new(2, 1, 0), out var b));
        Assert.Equal(a, dest.Orientation.Transform(dest.FlipMode.Transform(Vector3Int.zero, 2)) + dest.Coordinates);
        Assert.Equal(b, dest.Orientation.Transform(dest.FlipMode.Transform(new Vector3Int(1, 0, 0), 2)) + dest.Coordinates);
    }

    [Fact]
    public void MapPlacementOutsideAreaKeepsOrientation()
    {
        var t = MapTransform.RotateCw(Terrain, Total, 1, transformArea: true, new Vector2Int(1, 1), 2);
        var src = new Placement(new Vector3Int(0, 0, 0), Orientation.Cw0, FlipMode.Unflipped);

        Assert.True(t.TryMapPlacement(src, new Vector3Int(1, 1, 1), out var dest));
        Assert.Equal(new Vector3Int(0, 0, 0), dest.Coordinates);
        Assert.Equal(Orientation.Cw0, dest.Orientation);
    }
}
