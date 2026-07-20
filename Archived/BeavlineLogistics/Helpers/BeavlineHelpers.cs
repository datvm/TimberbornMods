namespace BeavlineLogistics.Helpers;

public static class BeavlineHelpers
{

    public static FrozenSet<BlockObject> FindAdjacentBlockObjects(this IBlockService blockService, BlockObject blockObject)
    {
        var startingBlocks = blockObject.PositionedBlocks.GetAllBlocks();

        HashSet<BlockObject> blockObjs = [];
        HashSet<Vector3Int> coords = [];
        foreach (var c in startingBlocks)
        {
            coords.Add(c.Coordinates);
        }

        // Enlist all block objects, filter later
        var neighbors = Deltas.Neighbors6Vector3Int;
        foreach (var c in startingBlocks)
        {
            foreach (var delta in neighbors)
            {
                var coord = c.Coordinates + delta;
                if (coords.Contains(coord)) { continue; }
                coords.Add(coord);

                var objs = blockService.GetObjectsAt(coord);
                foreach (var obj in objs)
                {
                    blockObjs.Add(obj);
                }
            }
        }

        return [..blockObjs];
    }

}
