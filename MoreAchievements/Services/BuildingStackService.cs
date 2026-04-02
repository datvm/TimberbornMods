namespace MoreAchievements.Services;

[BindSingleton]
public class BuildingStackService(
    IBlockService blocks,
    EntityRegistry entityRegistry
)
{

    public List<BlockObject> GetStackedBuildings(BlockObject topObj, int max)
        => GetStackedBuildings(topObj, max, static _ => true);

    public List<BlockObject> GetStackedBuildings(BlockObject topObj, int max, Func<BlockObject, bool> extraFilter)
    {
        List<BlockObject> result = [];

        if (max <= 0) { return result; }
        if (!extraFilter(topObj)) { return result; }

        var templateName = topObj.GetTemplateName();
        var footprint = GetFootprint(topObj);

        result.Add(topObj);
        if (result.Count >= max) { return result; }

        var curr = topObj;
        while (true)
        {
            var coord = curr.Coordinates;
            var coordBelow = coord with { z = coord.z - 1 };

            BlockObject? belowObj = null;
            foreach (var obj in blocks.GetObjectsAt(coordBelow))
            {
                if (!obj.IsFinished) { continue; }

                if (obj.GetTemplateName() != templateName) { continue; }
                if (!extraFilter(obj)) { continue; }

                var belowFootprint = GetFootprint(obj);
                if (!belowFootprint.SetEquals(footprint)) { continue; }

                belowObj = obj;
                break;
            }

            if (!belowObj) { break; }

            result.Add(belowObj!);
            if (result.Count >= max) { break; }
            curr = belowObj!;
        }

        return result;
    }

    /// <summary>
    /// Scans through all buildings in the world and finds stacks of buildings that meet the given criteria.
    /// Each stack is returned as a list of BlockObjects, ordered from top to bottom.
    /// </summary>
    /// <remarks>
    /// A building/stack may be returned multiple times if later a higher building in the stack is considered.
    /// </remarks>
    public IEnumerable<List<BlockObject>> ScanForStackBuildings(Func<BlockObject, bool> isValidConsideration, Func<BlockObject, bool> extraFilter)
    {
        HashSet<BlockObject> seen = [];

        foreach (var e in entityRegistry.Entities.Reverse()) // Greedy because the higher entity is more likely to be at the top
        {
            var bo = e.GetComponent<BlockObject>();
            if (!bo || seen.Contains(bo) || !isValidConsideration(bo)) { continue; }

            var stack = GetStackedBuildings(bo, int.MaxValue, extraFilter);
            if (stack.Count == 0) { continue; }

            foreach (var block in stack)
            {
                seen.Add(block);
            }
            yield return stack;
        }
    }

    static HashSet<Vector2Int> GetFootprint(BlockObject bo)
    {
        HashSet<Vector2Int> result = [];

        foreach (var c in bo.PositionedBlocks.GetAllCoordinates())
        {
            result.Add(c.XY());
        }

        return result;
    }

}
