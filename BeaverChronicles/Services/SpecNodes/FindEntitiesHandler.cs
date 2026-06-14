namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class FindEntitiesHandler(
    FindEntityHelper findEntityHelper
) : NodeHandlerBase<FindEntitiesData>
{
    public override string ForType => "FindEntities";

    protected override string? InternalHandleNode(FindEntitiesData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var prefix = data.ResultName;
        if (string.IsNullOrEmpty(prefix))
        {
            throw new InvalidOperationException($"FindEntities node {node.Id} has empty ResultName.");
        }

        var maxCount = Math.Max(0, data.MaxCount);
        var foundEntities = FindEntities(data);
        var entities = data.ChooseRandom
            ? foundEntities.OrderBy(_ => Random.value).Take(maxCount).ToArray()
            : foundEntities.Take(maxCount).ToArray();
        var customParameters = controller.CurrentRecord.CustomParameters;

        customParameters[$"{prefix}_Count"] = entities.Length.ToString();

        for (int i = 0; i < entities.Length; i++)
        {
            customParameters[$"{prefix}_{i + 1}"] = entities[i];
        }

        if (entities.Length == 1)
        {
            customParameters[prefix] = entities[0];
        }

        return entities.Length == 0 ? data.NoneFoundNodeId : node.NextNodeId;
    }

    IEnumerable<string> FindEntities(FindEntitiesData data)
    {
        var areas = data.AreasBounds;

        if (data.CharacterType != CharacterType.Unknown)
        {
            foreach (var c in findEntityHelper.GetCharacters(data.CharacterType, areas))
            {
                yield return c.GetEntityId().ToString();
            }
        }

        foreach (var c in findEntityHelper.FindEntitiesByTemplates(data.TemplateNames, data.TemplatePrefixes, areas, data.AreaCondition))
        {
            yield return c.GetEntityId().ToString();
        }
    }
}
