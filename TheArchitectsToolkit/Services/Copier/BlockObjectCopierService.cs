global using Timberborn.ToolSystem;
using Timberborn.Coordinates;

namespace TheArchitectsToolkit.Services.Copier;

public class BlockObjectCopierService(
    MapSize mapSize,
    IBlockService blockService,
    NaturalResourceSpawner naturalResourceSpawner,
    ToolButtonService toolButtons
) : ILoadableSingleton
{

    readonly Dictionary<string, BlockObjectTool> blockObjectTools = [];

    public void Load()
    {
        ScanForObjectTools();
    }

    void ScanForObjectTools()
    {
        ScanAllButtons(null, btn =>
        {
            if (btn.Tool is BlockObjectTool blockObjTool)
            {
                var prefabSpec = blockObjTool.Prefab.GetComponentFast<PrefabSpec>();
                blockObjectTools[prefabSpec.Name] = blockObjTool;
            }
        });
    }

    void ScanAllButtons(Action<ToolGroupButton>? onGroupFound, Action<ToolButton>? onButtonFound)
    {
        foreach (var btn in toolButtons._rootButtons)
        {
            if (btn is ToolGroupButton grp)
            {
                onGroupFound?.Invoke(grp);
                if (onButtonFound is null) { continue; }

                ScanGroupButtons(grp, onButtonFound);
            }
            else if (btn is ToolButton toolBtn && onButtonFound is not null)
            {
                onButtonFound?.Invoke(toolBtn);
            }
        }
    }

    void ScanGroupButtons(ToolGroupButton grp, Action<ToolButton> onButtonFound)
    {
        foreach (var btn in grp._toolButtons)
        {
            onButtonFound.Invoke(btn);
        }
    }

    public void Copy(in Vector2Int oldSize)
    {
        int x0 = oldSize.x, y0 = oldSize.y;

        var newSize = mapSize.TotalSize;
        int x1 = newSize.x, y1 = newSize.y, z1 = newSize.z;

        for (int x = 0; x < x1; x++)
        {
            var (srcX, flipX) = GetEnlargeCoord(x0, x);

            for (int y = 0; y < y1; y++)
            {
                var (srcY, flipY) = GetEnlargeCoord(y0, y);

                if (x < x0 && y < y0) { continue; } // skip the original area

                for (int z = 0; z < z1; z++)
                {
                    var srcLoc = new Vector3Int(srcX, srcY, z);
                    var blockObjs = blockService.GetObjectsAt(srcLoc);

                    foreach (var obj in blockObjs)
                    {
                        if (obj.Coordinates != srcLoc) { continue; }

                        Debug.Log($"Copying {obj.name} from ({srcX}, {srcY}, {z}) to ({x}, {y}, {z})");

                        TryPlacingObject(obj, new(x, y, z), flipX, flipY);
                    }
                }
            }
        }
    }

    bool TryPlacingObject(BlockObject obj, in Vector3Int location, bool flipX, bool flipY)
    {
        var spec = obj.GetComponentFast<PrefabSpec>();
        if (!spec) { return false; }

        if (spec.Name == "StartingLocation")
        {
            // Just skip this
            return true;
        }

        if (TryPlacingNaturalResources(obj, spec, location)) { return true; }
        if (TryPlacingWithTools(obj, spec, location, flipX, flipY)) { return true; }

        Debug.LogWarning("Copier does not know how to copy " + spec.name);
        return false;
    }

    bool TryPlacingNaturalResources(BlockObject blockObject, PrefabSpec prefab, in Vector3Int location)
    {
        if (!blockObject.GetComponentFast<NaturalResourceSpec>()) { return false; }

        var isMature = blockObject.GetComponentFast<Growable>().IsGrown;
        naturalResourceSpawner.Spawn([
            new(prefab.PrefabName, !isMature)],
            location);

        return true;
    }

    bool TryPlacingWithTools(BlockObject blockObject, PrefabSpec prefab, in Vector3Int location, bool flipX, bool flipY)
    {
        if (!blockObjectTools.TryGetValue(prefab.Name, out var tool)) { return false; }

        var originalPlacement = blockObject.Placement;
        var orientation = originalPlacement.Orientation;

        if ((flipY && orientation is Orientation.Cw0 or Orientation.Cw180) 
            || (flipX && orientation is Orientation.Cw90 or Orientation.Cw270))
        {
            orientation = orientation.Flip();
        }

        var placement = new Placement(
            location,
            orientation,
            originalPlacement.FlipMode
        );

        tool.Place([placement]);

        return true;
    }

    static (int coord, bool flip) GetEnlargeCoord(int oldSize, int x)
    {
        if (oldSize == 1) { return (0, false); }

        if (x < oldSize) { return (x, false); }

        var period = 2 * oldSize;
        var mod = x % period;
        var flip = mod >= oldSize;
        return ((mod < oldSize) ? mod : period - mod - 1, flip);
    }

}
