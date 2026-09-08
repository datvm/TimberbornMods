namespace MapTransformer.Transform;

static class MapIndexCopy
{
    public static void CopyTerrainVoxels(
        bool[] oldVoxels,
        bool[] newVoxels,
        in MapTransformContext ctx)
    {
        var t = ctx.Transform;
        var size = t.NewTerrainSize;
        for (var y = 0; y < size.y; y++)
        {
            for (var x = 0; x < size.x; x++)
            {
                for (var z = 0; z < size.z - 1; z++)
                {
                    var dest = new Vector3Int(x, y, z);
                    if (!t.TryUnmapOrFill(dest, out var src))
                    {
                        continue;
                    }

                    if (src.z < 0 || src.z >= t.OldTerrainSize.z)
                    {
                        continue;
                    }

                    newVoxels[ctx.MapIndex.CoordinatesToIndex3D(dest)] =
                        oldVoxels[ctx.OldMapIndex.CoordinatesToIndex3D(src)];
                }
            }
        }
    }

    public static void CopyColumns<T>(
        ReadOnlySpan<T> oldValues,
        int oldVerticalStride,
        ReadOnlySpan<byte> oldCounts,
        Span<T> newValues,
        int newVerticalStride,
        ReadOnlySpan<byte> newCounts,
        in MapTransformContext ctx)
    {
        var t = ctx.Transform;
        var size = t.NewTerrainSize;
        for (var y = 0; y < size.y; y++)
        {
            for (var x = 0; x < size.x; x++)
            {
                if (!t.TryUnmapOrFill(new Vector3Int(x, y, 0), out var src))
                {
                    continue;
                }

                if (src.x < 0 || src.y < 0 || src.x >= t.OldTerrainSize.x || src.y >= t.OldTerrainSize.y)
                {
                    continue;
                }

                var oldI = ctx.OldMapIndex.CellToIndex(new Vector2Int(src.x, src.y));
                var newI = ctx.MapIndex.CellToIndex(new Vector2Int(x, y));
                var n = Math.Min(oldCounts[oldI], newCounts[newI]);
                for (var col = 0; col < n; col++)
                {
                    newValues[col * newVerticalStride + newI] = oldValues[col * oldVerticalStride + oldI];
                }
            }
        }
    }

    public static void CopyColumnFloats(
        ReadOnlySpan<float> oldValues,
        int oldVerticalStride,
        int oldMaxColumns,
        Span<float> newValues,
        int newVerticalStride,
        int newMaxColumns,
        in MapTransformContext ctx)
    {
        var t = ctx.Transform;
        var size = t.NewTerrainSize;
        for (var y = 0; y < size.y; y++)
        {
            for (var x = 0; x < size.x; x++)
            {
                if (!t.TryUnmapOrFill(new Vector3Int(x, y, 0), out var src))
                {
                    continue;
                }

                if (src.x < 0 || src.y < 0 || src.x >= t.OldTerrainSize.x || src.y >= t.OldTerrainSize.y)
                {
                    continue;
                }

                var oldI = ctx.OldMapIndex.CellToIndex(new Vector2Int(src.x, src.y));
                var newI = ctx.MapIndex.CellToIndex(new Vector2Int(x, y));
                var n = Math.Min(oldMaxColumns, newMaxColumns);
                for (var col = 0; col < n; col++)
                {
                    var oldIndex = col * oldVerticalStride + oldI;
                    var newIndex = col * newVerticalStride + newI;
                    if (oldIndex < oldValues.Length && newIndex < newValues.Length)
                    {
                        newValues[newIndex] = oldValues[oldIndex];
                    }
                }
            }
        }
    }
}
