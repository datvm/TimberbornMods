namespace ConfigurablePlants.Services;

public class NoGroundPlantTerrainService(
    CursorCoordinatesPicker cursor
) : ILoadableSingleton, IUnloadableSingleton
{

    public static NoGroundPlantTerrainService? Instance { get; private set; }

    public void Load()
    {
        Instance = this;
    }

    public static IEnumerable<Vector3Int> ModifiedGetTerrainBlocks(Ray startRay, Ray endRay, AreaPicker instance)
    {
        var cursor = Instance;
        if (cursor is null) { return []; }

        var startCoord = cursor.TryGetCursorCoordinates(startRay);
        if (startCoord is null) { return []; }

        var endCoord = cursor.TryGetCursorCoordinates(endRay);
        if (endCoord is null) { return []; }

        var end = endCoord.Value with { z = startCoord.Value.z, };
        var blocks = instance._areaIterator.GetRectangle(startCoord.Value, end, instance._maxBlocks);

        return blocks;
    }

    public Vector3Int? TryGetCursorCoordinates(Ray ray)
    {
        // Copy source code of CursorCoordinates, unfortunately because it doesn't have any parameter for an input ray
        if (TryGetBlockObjectCoordinates(ray, out var foundCoordinates))
        {
            return foundCoordinates!.Value.TileCoordinates;
        }

        TraversedCoordinates? traversedCoordinates = cursor._terrainPicker.PickTerrainCoordinates(ray);
        if (traversedCoordinates.HasValue)
        {
            TraversedCoordinates valueOrDefault = traversedCoordinates.GetValueOrDefault();
            Vector3Int vector3Int = valueOrDefault.Coordinates + valueOrDefault.Face;
            if (cursor._terrainService.Contains(vector3Int))
            {
                return vector3Int;
            }
        }

        return null;
    }

    bool TryGetBlockObjectCoordinates(Ray ray, out CursorCoordinates? foundCoordinates)
    {
        if (TryHitSelectableObject(cursor._selectableObjectRaycaster, ray, out var hitObject))
        {
            var block = hitObject!.GetComponent<BlockObject>();
            if (block is not null && !block.IsPreview)
            {
                foundCoordinates = CursorCoordinatesPicker.GetFloorOrPathCoordinatesHit(ray, block)
                    ?? cursor.GetStackableCoordinatesHit(ray, block);
                return foundCoordinates.HasValue;
            }
        }
        foundCoordinates = null;
        return false;
    }

    bool TryHitSelectableObject(SelectableObjectRaycaster raycaster, Ray ray, out SelectableObject? hitObject)
    {
        // Copy source code of SelectableObjectRaycaster, unfortunately because it doesn't have any parameter for an input ray

        ray = CoordinateSystem.GridToWorld(ray);
        return raycaster.TryHitSelectableObject(ray, false, out hitObject, out _);
    }



    public void Unload()
    {
        Instance = null;
    }
}
