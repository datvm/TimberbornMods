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
        var foundEntities = FindEntities(data, controller);
        string[] entities = [..data.ChooseRandom
            ? foundEntities.OrderBy(_ => Random.value).Take(maxCount)
            : foundEntities.Take(maxCount)];
        var customParameters = controller.CurrentRecord.CustomParameters;

        customParameters[$"{prefix}_Count"] = entities.Length.ToString();
        var nextNodeId = entities.Length == 0 ? data.NoneFoundNodeId : node.NextNodeId;
        node.LogVerbose(() => $"Found {entities.Length} entities. Going to {nextNodeId}");

        for (int i = 0; i < entities.Length; i++)
        {
            customParameters[$"{prefix}_{i + 1}"] = entities[i];
        }

        if (entities.Length == 1)
        {
            customParameters[prefix] = entities[0];
        }

        return nextNodeId;
    }

    IEnumerable<string> FindEntities(FindEntitiesData data, SpecChronicleEventController controller)
    {
        var areas = data.AreasBounds;

        if (data.CharacterType != CharacterType.Unknown)
        {
            foreach (var c in findEntityHelper.GetCharacters(data.CharacterType, areas))
            {
                yield return c.GetEntityId().ToString();
            }
        }

        var templateNames = controller.FormatTextsRemoveEmpty(data.TemplateNames).ToArray();
        var templatePrefixes = controller.FormatTextsRemoveEmpty(data.TemplatePrefixes).ToArray();

        foreach (var c in findEntityHelper.FindEntitiesByTemplates(templateNames, templatePrefixes, areas, data.AreaCondition))
        {
            yield return c.GetEntityId().ToString();
        }
    }
}
