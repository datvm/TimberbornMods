namespace ScenarioEditor.Services;

public class TriggerAreaService(
    EntityRegistry entities
)
{

    public IReadOnlyList<EntityComponent>? CheckForArea(AreaTrigger areaTrigger)
    {
        List<EntityComponent> result = [];

        var targets = areaTrigger.GameObjects;
        var areas = areaTrigger.Areas;
        var required = areaTrigger.MinimumObjectsCount;

        var checkCharacters = targets.ChildBeaver || targets.AdultBeaver || targets.Bot;
        var checkForAnyBlockObject = targets.AnyBlockObject;
        var checkForBlockObjects = checkForAnyBlockObject || targets.SpecificObject is not null;
        var finishedOnly = targets.FinishedOnly;
        var prefabName = targets.SpecificObject;

        foreach (var entity in entities.Entities)
        {
            if (!entity || entity.Deleted) { continue; }

            if (checkCharacters && IsCharacterInArea(entity, targets, areas))
            {
                result.Add(entity);
            }
            else if (checkForBlockObjects && IsBlockObjectInArea(entity, finishedOnly, prefabName, areas))
            {
                result.Add(entity);
            }

            if (result.Count >= required)
            {
                return result;
            }
        }

        return null;
    }

    bool IsCharacterInArea(EntityComponent entity, GameObjectDefinition targets, List<AreaDefinition> areas)
    {
        if ((!targets.AdultBeaver || !entity.GetComponentFast<AdultSpec>())
            && (!targets.ChildBeaver || !entity.GetComponentFast<ChildSpec>())
            && (!targets.Bot || !entity.GetComponentFast<BotSpec>()))
        {
            return false;
        }

        var pos = entity.TransformFast.position;
        return areas.FastAny(q => IsInArea(pos, q));
    }

    bool IsBlockObjectInArea(EntityComponent entity, bool finishedOnly, string? prefabName, List<AreaDefinition> areas)
    {
        if (prefabName is not null)
        {
            var prefabSpec = entity.GetComponentFast<PrefabSpec>();
            if (!prefabSpec || prefabSpec.Name != prefabName) { return false; }
        }

        var blockObj = entity.GetComponentFast<BlockObject>();
        if (!blockObj
            || (finishedOnly && !blockObj.IsFinished)) { return false; }

        return blockObj.Blocks.GetOccupiedBlocks().Any(block => areas.Any(area => IsInArea(block.Coordinates, area)));
    }

    static bool IsInArea(in Vector3 pos, in AreaDefinition area) =>
        pos.x >= area.Start.x && pos.x <= area.End.x
        && pos.y >= area.Start.y && pos.y <= area.End.y
        && pos.z >= area.Start.z && pos.z <= area.End.z;

}
