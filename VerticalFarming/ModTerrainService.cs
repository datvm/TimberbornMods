global using Timberborn.CursorToolSystem;
global using Timberborn.GridTraversing;
using Timberborn.AreaSelectionSystem;
using Timberborn.Coordinates;
using Timberborn.SelectionSystem;

namespace VerticalFarming;

public class ModTerrainService : IUnloadableSingleton
{
    public static ModTerrainService? Instance { get; private set; }

    public CursorCoordinatesPicker Cursor { get; private set; }

    public ModTerrainService(CursorCoordinatesPicker cursor)
    {
        Cursor = cursor;

        Instance = this;
    }

    /// <summary>
    /// Modified the original game code to get blocks top as well
    /// </summary>
    public static IEnumerable<Vector3Int> ModifiedGetTerrainBlocks(Ray startRay, Ray endRay, AreaPicker instance)
    {
        var cursor = Instance;
        if (cursor is null) { return []; }

        var startCoord = cursor.TryGetCursorCoordinates(startRay);
        if (startCoord is null) { return []; }

        var endCoord = cursor.TryGetCursorCoordinates(endRay);
        if (endCoord is null) { return []; }

        var end = endCoord.Value with { z = startCoord.Value.z, };
        var blocks = instance._areaIterator.GetRectangle(startCoord.Value, end, AreaPicker.MaxBlocksReturned);

        return blocks;
    }

    public Vector3Int? TryGetCursorCoordinates(Ray ray)
    {
        // Copy source code of CursorCoordinates, unfortunately because it doesn't have any parameter for an input ray
        if (TryGetBlockObjectCoordinates(ray, out var foundCoordinates))
        {
            return foundCoordinates!.Value.TileCoordinates;
        }

        TraversedCoordinates? traversedCoordinates = Cursor._terrainPicker.PickTerrainCoordinates(ray);
        if (traversedCoordinates.HasValue)
        {
            TraversedCoordinates valueOrDefault = traversedCoordinates.GetValueOrDefault();
            Vector3Int vector3Int = valueOrDefault.Coordinates + valueOrDefault.Face;
            if (Cursor._terrainService.Contains(vector3Int))
            {
                return vector3Int;
            }
        }

        return null;
    }

    bool TryGetBlockObjectCoordinates(Ray ray, out CursorCoordinates? foundCoordinates)
    {
        if (TryHitSelectableObject(Cursor._selectableObjectRaycaster, ray, out var hitObject))
        {
            var block = hitObject!.GetComponentFast<BlockObject>();
            if (block is not null && !block.IsPreview)
            {
                foundCoordinates = CursorCoordinatesPicker.GetFloorOrPathCoordinatesHit(ray, block)
                    ?? Cursor.GetStackableCoordinatesHit(ray, block);
                return foundCoordinates.HasValue;
            }
        }
        foundCoordinates = null;
        return false;
    }

    bool TryHitSelectableObject(SelectableObjectRaycaster raycaster, Ray ray, out BaseComponent? hitObject)
    {
        // Copy source code of SelectableObjectRaycaster, unfortunately because it doesn't have any parameter for an input ray

        ray = CoordinateSystem.GridToWorld(ray);

        if (Physics.Raycast(ray, out var hitInfo) && raycaster.HitIsCloserThanTerrain(ray, hitInfo))
        {
            GameObject gameObject = hitInfo.collider.gameObject;
            if ((bool)gameObject)
            {
                hitObject = raycaster._selectableObjectRetriever.GetSelectableObject(gameObject);
                return true;
            }
        }
        hitObject = null;
        return false;
    }

    public void Unload()
    {
        Instance = null;
    }
}
