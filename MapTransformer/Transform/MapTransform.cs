namespace MapTransformer.Transform;

readonly record struct MapTransform
{
    public MapOp Op { get; init; }
    public Vector3Int OldTerrainSize { get; init; }
    public Vector3Int NewTerrainSize { get; init; }
    public Vector3Int OldTotalSize { get; init; }
    public Vector3Int NewTotalSize { get; init; }
    public Vector2Int PivotOffset { get; init; }
    public EnlargeFill Fill { get; init; }
    public int RotateCwSteps { get; init; }
    public bool FlipX { get; init; }
    public bool FlipY { get; init; }
    public int InsertLayers { get; init; }
    public bool TransformArea { get; init; }
    public Vector2Int AreaOrigin { get; init; }
    public int AreaSize { get; init; }

    public bool HasArea => TransformArea;

    public static MapTransform ResizeXy(
        Vector3Int oldTerrain,
        Vector3Int oldTotal,
        Vector2Int newXy,
        Vector2Int pivot,
        EnlargeFill fill)
    {
        var x = Math.Max(1, newXy.x);
        var y = Math.Max(1, newXy.y);
        return new()
        {
            Op = MapOp.ResizeXy,
            OldTerrainSize = oldTerrain,
            NewTerrainSize = new Vector3Int(x, y, oldTerrain.z),
            OldTotalSize = oldTotal,
            NewTotalSize = new Vector3Int(x, y, oldTotal.z),
            PivotOffset = new Vector2Int(
                AnchorOffset(oldTerrain.x, x, pivot.x),
                AnchorOffset(oldTerrain.y, y, pivot.y)),
            Fill = fill,
        };
    }

    public static MapTransform ResizeHeight(Vector3Int oldTerrain, Vector3Int oldTotal, int terrainZ, int totalZ)
    {
        terrainZ = Math.Max(1, terrainZ);
        totalZ = Math.Max(terrainZ, totalZ);
        return new()
        {
            Op = MapOp.ResizeHeight,
            OldTerrainSize = oldTerrain,
            NewTerrainSize = new Vector3Int(oldTerrain.x, oldTerrain.y, terrainZ),
            OldTotalSize = oldTotal,
            NewTotalSize = new Vector3Int(oldTotal.x, oldTotal.y, totalZ),
        };
    }

    public static MapTransform AddHeight(Vector3Int oldTerrain, Vector3Int oldTotal, int layers, bool increaseTerrainHeight)
    {
        layers = Math.Max(0, layers);
        var grow = increaseTerrainHeight ? layers : 0;
        return new()
        {
            Op = MapOp.AddHeight,
            OldTerrainSize = oldTerrain,
            NewTerrainSize = oldTerrain + new Vector3Int(0, 0, grow),
            OldTotalSize = oldTotal,
            NewTotalSize = oldTotal + new Vector3Int(0, 0, grow),
            InsertLayers = layers,
        };
    }

    public static MapTransform RotateCw(
        Vector3Int oldTerrain,
        Vector3Int oldTotal,
        int steps,
        bool transformArea = false,
        Vector2Int areaOrigin = default,
        int areaSize = 0)
    {
        steps = NormalizeSteps(steps);
        var swap = !transformArea && steps is 1 or 3;
        return new()
        {
            Op = MapOp.Rotate,
            OldTerrainSize = oldTerrain,
            NewTerrainSize = SwapXy(oldTerrain, swap),
            OldTotalSize = oldTotal,
            NewTotalSize = SwapXy(oldTotal, swap),
            RotateCwSteps = steps,
            TransformArea = transformArea,
            AreaOrigin = areaOrigin,
            AreaSize = areaSize,
        };
    }

    public static MapTransform Flip(
        Vector3Int oldTerrain,
        Vector3Int oldTotal,
        bool flipX,
        bool flipY,
        bool transformArea = false,
        Vector2Int areaOrigin = default,
        int areaSize = 0)
    {
        return new()
        {
            Op = MapOp.Flip,
            OldTerrainSize = oldTerrain,
            NewTerrainSize = oldTerrain,
            OldTotalSize = oldTotal,
            NewTotalSize = oldTotal,
            FlipX = flipX,
            FlipY = flipY,
            TransformArea = transformArea,
            AreaOrigin = areaOrigin,
            AreaSize = areaSize,
        };
    }

    public string? ErrorLocKey()
    {
        if (Op is not (MapOp.Rotate or MapOp.Flip) || !TransformArea)
        {
            return null;
        }

        if (AreaOrigin.x < 0
            || AreaOrigin.y < 0
            || AreaSize < 1
            || AreaOrigin.x + AreaSize > OldTerrainSize.x
            || AreaOrigin.y + AreaSize > OldTerrainSize.y)
        {
            return "LV.MTr.AreaExceedsMap";
        }

        return null;
    }

    public bool TryMapHeight(int srcZ, out int destZ) => TryMapZ(srcZ, out destZ);

    public bool TryMapCell(Vector3Int src, out Vector3Int dest)
    {
        dest = default;
        if (!TryMap(src, out dest))
        {
            return false;
        }

        return Contains(dest, NewTotalSize);
    }

    public bool TryUnmapCell(Vector3Int dest, out Vector3Int src)
    {
        src = default;
        if (!TryUnmap(dest, out src))
        {
            return false;
        }

        return Contains(src, OldTotalSize);
    }

    public bool TryUnmapOrFill(Vector3Int dest, out Vector3Int src)
    {
        if (TryUnmapCell(dest, out src))
        {
            if ((Op is MapOp.ResizeXy or MapOp.ResizeHeight) && dest.z >= OldTerrainSize.z)
            {
                return false;
            }

            return true;
        }

        if (Op != MapOp.ResizeXy || Fill == EnlargeFill.Empty)
        {
            return false;
        }

        if (dest.z < 0 || dest.z >= OldTerrainSize.z)
        {
            return false;
        }

        var x = FillAxis(dest.x, PivotOffset.x, OldTerrainSize.x);
        var y = FillAxis(dest.y, PivotOffset.y, OldTerrainSize.y);
        if (x < 0 || y < 0)
        {
            return false;
        }

        src = new Vector3Int(x, y, dest.z);
        return Contains(src, OldTerrainSize) || Contains(src, OldTotalSize);
    }

    public bool TryUnmapColumn(Vector2Int dest, out Vector2Int src)
    {
        var probeZ = Op == MapOp.AddHeight
            ? Math.Min(InsertLayers, Math.Max(0, NewTotalSize.z - 1))
            : 0;
        if (!TryUnmapOrFill(new Vector3Int(dest.x, dest.y, probeZ), out var cell))
        {
            src = default;
            return false;
        }

        src = new Vector2Int(cell.x, cell.y);
        return src.x >= 0 && src.y >= 0 && src.x < OldTerrainSize.x && src.y < OldTerrainSize.y;
    }

    public bool TryMapPlacement(Placement src, Vector3Int localSize, out Placement dest)
    {
        dest = default;
        var mapped = new HashSet<Vector3Int>();
        foreach (var local in EnumerateBox(localSize))
        {
            var world = WorldOfLocal(src, local, localSize);
            if (!TryMapCell(world, out var mappedCell))
            {
                return false;
            }

            mapped.Add(mappedCell);
        }

        if (mapped.Count == 0)
        {
            return false;
        }

        var predicted = PredictPlacement(src);
        if (TryPlacementFromMapped(mapped, localSize, predicted.Orientation, predicted.FlipMode, out dest))
        {
            return true;
        }

        foreach (var orientation in OrientationExtensions.AllValues())
        {
            foreach (var flip in new FlipMode[] { FlipMode.Unflipped, FlipMode.Flipped })
            {
                if (orientation == predicted.Orientation && flip.Equals(predicted.FlipMode))
                {
                    continue;
                }

                if (TryPlacementFromMapped(mapped, localSize, orientation, flip, out dest))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Vector3 MapWorld(Vector3 world)
    {
        var grid = CoordinateSystem.WorldToGrid(world);
        var cell = new Vector3Int(
            Mathf.FloorToInt(grid.x),
            Mathf.FloorToInt(grid.y),
            Mathf.FloorToInt(grid.z));
        var frac = grid - new Vector3(cell.x, cell.y, cell.z);
        if (!TryMapCell(cell, out var dest))
        {
            if (!TryMap(cell, out dest))
            {
                dest = cell;
            }

            dest = new Vector3Int(
                Math.Clamp(dest.x, 0, Math.Max(0, NewTotalSize.x - 1)),
                Math.Clamp(dest.y, 0, Math.Max(0, NewTotalSize.y - 1)),
                Math.Clamp(dest.z, 0, Math.Max(0, NewTotalSize.z - 1)));
        }

        return CoordinateSystem.GridToWorld(new Vector3(dest.x, dest.y, dest.z) + frac);
    }

    bool TryMap(Vector3Int src, out Vector3Int dest)
    {
        dest = default;
        if (!TryMapZ(src.z, out var z))
        {
            return false;
        }

        MapXy(src.x, src.y, out var x, out var y);
        dest = new Vector3Int(x, y, z);
        return true;
    }

    bool TryUnmap(Vector3Int dest, out Vector3Int src)
    {
        src = default;
        if (!TryUnmapZ(dest.z, out var z))
        {
            return false;
        }

        UnmapXy(dest.x, dest.y, out var x, out var y);
        src = new Vector3Int(x, y, z);
        return true;
    }

    void MapXy(int x, int y, out int nx, out int ny)
    {
        switch (Op)
        {
            case MapOp.ResizeXy:
                nx = x + PivotOffset.x;
                ny = y + PivotOffset.y;
                return;
            case MapOp.Rotate:
                MapRotate(x, y, out nx, out ny);
                return;
            case MapOp.Flip:
                MapFlip(x, y, out nx, out ny);
                return;
            default:
                nx = x;
                ny = y;
                return;
        }
    }

    void UnmapXy(int nx, int ny, out int x, out int y)
    {
        switch (Op)
        {
            case MapOp.ResizeXy:
                x = nx - PivotOffset.x;
                y = ny - PivotOffset.y;
                return;
            case MapOp.Rotate:
                UnmapRotate(nx, ny, out x, out y);
                return;
            case MapOp.Flip:
                UnmapFlip(nx, ny, out x, out y);
                return;
            default:
                x = nx;
                y = ny;
                return;
        }
    }

    void MapRotate(int x, int y, out int nx, out int ny)
    {
        if (HasArea)
        {
            if (!InArea(x, y))
            {
                nx = x;
                ny = y;
                return;
            }

            RotateXy(x - AreaOrigin.x, y - AreaOrigin.y, AreaSize, AreaSize, RotateCwSteps, out var lx, out var ly);
            nx = AreaOrigin.x + lx;
            ny = AreaOrigin.y + ly;
            return;
        }

        RotateXy(x, y, OldTerrainSize.x, OldTerrainSize.y, RotateCwSteps, out nx, out ny);
    }

    void UnmapRotate(int nx, int ny, out int x, out int y)
    {
        if (HasArea)
        {
            if (!InArea(nx, ny))
            {
                x = nx;
                y = ny;
                return;
            }

            UnrotateXy(nx - AreaOrigin.x, ny - AreaOrigin.y, AreaSize, AreaSize, RotateCwSteps, out var lx, out var ly);
            x = AreaOrigin.x + lx;
            y = AreaOrigin.y + ly;
            return;
        }

        UnrotateXy(nx, ny, OldTerrainSize.x, OldTerrainSize.y, RotateCwSteps, out x, out y);
    }

    void MapFlip(int x, int y, out int nx, out int ny)
    {
        if (HasArea)
        {
            if (!InArea(x, y))
            {
                nx = x;
                ny = y;
                return;
            }

            var lx = x - AreaOrigin.x;
            var ly = y - AreaOrigin.y;
            nx = AreaOrigin.x + (FlipX ? AreaSize - 1 - lx : lx);
            ny = AreaOrigin.y + (FlipY ? AreaSize - 1 - ly : ly);
            return;
        }

        nx = FlipX ? OldTerrainSize.x - 1 - x : x;
        ny = FlipY ? OldTerrainSize.y - 1 - y : y;
    }

    void UnmapFlip(int nx, int ny, out int x, out int y)
        => MapFlip(nx, ny, out x, out y);

    bool InArea(int x, int y)
        => x >= AreaOrigin.x
            && y >= AreaOrigin.y
            && x < AreaOrigin.x + AreaSize
            && y < AreaOrigin.y + AreaSize;

    bool TryMapZ(int z, out int destZ)
    {
        destZ = z;
        if (Op != MapOp.AddHeight)
        {
            return true;
        }

        destZ = z + InsertLayers;
        return destZ >= 0 && destZ < NewTotalSize.z;
    }

    bool TryUnmapZ(int destZ, out int srcZ)
    {
        srcZ = destZ;
        if (Op != MapOp.AddHeight)
        {
            return true;
        }

        srcZ = destZ - InsertLayers;
        return srcZ >= 0;
    }

    int FillAxis(int dest, int offset, int oldSize)
    {
        var local = dest - offset;
        if (local >= 0 && local < oldSize)
        {
            return local;
        }

        if (oldSize <= 0)
        {
            return -1;
        }

        if (Fill == EnlargeFill.CopyEdge)
        {
            return Math.Clamp(local, 0, oldSize - 1);
        }

        if (oldSize == 1)
        {
            return 0;
        }

        var period = 2 * oldSize;
        var m = local % period;
        if (m < 0)
        {
            m += period;
        }

        return m < oldSize ? m : period - m - 1;
    }

    Placement PredictPlacement(Placement src)
    {
        var orientation = src.Orientation;
        var flip = src.FlipMode;
        var inArea = !HasArea || InArea(src.Coordinates.x, src.Coordinates.y);
        if (Op == MapOp.Rotate && inArea)
        {
            for (var i = 0; i < RotateCwSteps; i++)
            {
                orientation = orientation.RotateClockwise();
            }
        }

        if (Op == MapOp.Flip && inArea)
        {
            if (FlipX)
            {
                flip = flip.Flip();
            }

            if (FlipY)
            {
                orientation = orientation.Flip();
            }
        }

        return new Placement(src.Coordinates, orientation, flip);
    }

    static bool TryPlacementFromMapped(
        HashSet<Vector3Int> mapped,
        Vector3Int localSize,
        Orientation orientation,
        FlipMode flip,
        out Placement dest)
    {
        dest = default;
        var footprintMin = ComponentMin(EnumerateBox(localSize)
            .Select(local => orientation.Transform(flip.Transform(local, localSize.x))));
        var mappedMin = ComponentMin(mapped);
        var coords = mappedMin - footprintMin;
        var candidate = new Placement(coords, orientation, flip);
        var occupied = new HashSet<Vector3Int>();
        foreach (var local in EnumerateBox(localSize))
        {
            occupied.Add(WorldOfLocal(candidate, local, localSize));
        }

        if (!occupied.SetEquals(mapped))
        {
            return false;
        }

        dest = candidate;
        return true;
    }

    static Vector3Int WorldOfLocal(Placement placement, Vector3Int local, Vector3Int localSize)
        => placement.Orientation.Transform(placement.FlipMode.Transform(local, localSize.x)) + placement.Coordinates;

    static IEnumerable<Vector3Int> EnumerateBox(Vector3Int size)
    {
        for (var x = 0; x < Math.Max(1, size.x); x++)
        {
            for (var y = 0; y < Math.Max(1, size.y); y++)
            {
                for (var z = 0; z < Math.Max(1, size.z); z++)
                {
                    yield return new Vector3Int(x, y, z);
                }
            }
        }
    }

    static Vector3Int ComponentMin(IEnumerable<Vector3Int> cells)
    {
        var min = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        foreach (var cell in cells)
        {
            min = new Vector3Int(Math.Min(min.x, cell.x), Math.Min(min.y, cell.y), Math.Min(min.z, cell.z));
        }

        return min;
    }

    static bool Contains(Vector3Int p, Vector3Int size)
        => p.x >= 0 && p.y >= 0 && p.z >= 0 && p.x < size.x && p.y < size.y && p.z < size.z;

    static Vector3Int SwapXy(Vector3Int size, bool swap)
        => swap ? new Vector3Int(size.y, size.x, size.z) : size;

    static int AnchorOffset(int oldSize, int newSize, int pivot)
    {
        if (oldSize <= 0)
        {
            return 0;
        }

        pivot = Math.Clamp(pivot, 0, oldSize);
        return (int)((long)pivot * newSize / oldSize) - pivot;
    }

    static int NormalizeSteps(int steps)
    {
        steps %= 4;
        if (steps < 0)
        {
            steps += 4;
        }

        return steps;
    }

    static void RotateXy(int x, int y, int oldW, int oldH, int steps, out int nx, out int ny)
    {
        (nx, ny) = NormalizeSteps(steps) switch
        {
            1 => (y, oldW - 1 - x),
            2 => (oldW - 1 - x, oldH - 1 - y),
            3 => (oldH - 1 - y, x),
            _ => (x, y),
        };
    }

    static void UnrotateXy(int nx, int ny, int oldW, int oldH, int steps, out int x, out int y)
    {
        (x, y) = NormalizeSteps(steps) switch
        {
            1 => (oldW - 1 - ny, nx),
            2 => (oldW - 1 - nx, oldH - 1 - ny),
            3 => (ny, oldH - 1 - nx),
            _ => (nx, ny),
        };
    }

}
