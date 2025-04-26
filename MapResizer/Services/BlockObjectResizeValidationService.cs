global using Timberborn.EntitySystem;

namespace MapResizer.Services;

public class BlockObjectResizeValidationService(
    EntityRegistry entities,
    EntityService entityService
)
{

    public bool ValidateBlockObjects(in Vector3Int totalSize, in Vector3Int terrainSize)
    {
        return !GetInvalidBlockObjects(totalSize, terrainSize).Any();
    }

    public void DeleteInvalidBlockObjects(in Vector3Int totalSize, in Vector3Int terrainSize)
    {
        foreach (var obj in GetInvalidBlockObjects(totalSize, terrainSize))
        {
            entityService.Delete(obj);
        }
    }

    IEnumerable<BlockObject> GetInvalidBlockObjects(Vector3Int totalSize, Vector3Int terrainSize)
    {
        foreach (var en in entities.Entities.ToArray())
        {
            var blockObj = en.GetComponentFast<BlockObject>();
            if (!blockObj) { continue; }

            if (
                !IsValid(blockObj.Coordinates, totalSize) ||
                blockObj.PositionedBlocks.GetAllBlocks()
                    .Any(q => !IsValid(
                        q.Coordinates, 
                        q.IsFoundationBlock ? terrainSize : totalSize)))
            {
                yield return blockObj;
            }
        }
    }

    static bool IsValid(in Vector3Int coord, in Vector3Int size)
    {
        return coord.x < size.x && coord.y < size.y && coord.z < size.z - 1;
    }

}
