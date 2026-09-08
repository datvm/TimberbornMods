namespace MapTransformer.Tests;

public class MapTransformTests
{
    static readonly Vector3Int Terrain = new(4, 6, 8);
    static readonly Vector3Int Total = new(4, 6, 18);

    [Fact]
    public void ResizeMinMinKeepsOrigin()
    {
        var t = MapTransform.Resize(Terrain, Total, new(6, 8, 10), new(6, 8, 20), MapPivot.MinMin, EnlargeFill.Empty);

        Assert.True(t.TryMapCell(new(0, 0, 3), out var dest));
        Assert.Equal(new Vector3Int(0, 0, 3), dest);
        Assert.True(t.TryUnmapCell(dest, out var src));
        Assert.Equal(new Vector3Int(0, 0, 3), src);
        Assert.False(t.TryUnmapCell(new(5, 0, 3), out _));
    }

    [Fact]
    public void ResizeMaxMaxShiftsOrigin()
    {
        var t = MapTransform.Resize(Terrain, Total, new(6, 8, 8), new(6, 8, 18), MapPivot.MaxMax, EnlargeFill.Empty);

        Assert.True(t.TryMapCell(new(0, 0, 1), out var dest));
        Assert.Equal(new Vector3Int(2, 2, 1), dest);
    }

    [Fact]
    public void ResizeZAddsEmptyUnmapMiss()
    {
        var t = MapTransform.Resize(Terrain, Total, new(4, 6, 12), new(4, 6, 22), MapPivot.MinMin, EnlargeFill.Empty);

        Assert.True(t.TryUnmapCell(new(1, 1, 3), out _));
        Assert.False(t.TryUnmapOrFill(new(1, 1, 10), out _));
    }

    [Fact]
    public void ResizeFillCopyEdge()
    {
        var t = MapTransform.Resize(Terrain, Total, new(6, 6, 8), new(6, 6, 18), MapPivot.MinMin, EnlargeFill.CopyEdge);

        Assert.True(t.TryUnmapOrFill(new(5, 0, 2), out var src));
        Assert.Equal(new Vector3Int(3, 0, 2), src);
    }

    [Fact]
    public void ResizeFillMirror()
    {
        var t = MapTransform.Resize(Terrain, Total, new(8, 6, 8), new(8, 6, 18), MapPivot.MinMin, EnlargeFill.Mirror);

        Assert.True(t.TryUnmapOrFill(new(4, 1, 2), out var src));
        Assert.Equal(new Vector3Int(3, 1, 2), src);
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
        var t = MapTransform.AddHeight(Terrain, Total, 2, 10);

        Assert.Equal(10, t.NewTerrainSize.z);
        Assert.True(t.TryMapCell(new(1, 1, 0), out var dest));
        Assert.Equal(new Vector3Int(1, 1, 2), dest);
        Assert.False(t.TryUnmapCell(new(1, 1, 1), out _));
        Assert.True(t.TryUnmapCell(new(1, 1, 2), out var src));
        Assert.Equal(new Vector3Int(1, 1, 0), src);
    }

    [Fact]
    public void RemoveHeightCompacts()
    {
        var t = MapTransform.RemoveHeight(Terrain, Total, 2, 3);

        Assert.Equal(6, t.NewTerrainSize.z);
        Assert.False(t.TryMapCell(new(0, 0, 2), out _));
        Assert.True(t.TryMapCell(new(0, 0, 4), out var dest));
        Assert.Equal(new Vector3Int(0, 0, 2), dest);
        Assert.True(t.TryUnmapCell(dest, out var src));
        Assert.Equal(new Vector3Int(0, 0, 4), src);
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
}
