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
        IEnumerable<BaseComponent> ordered = data.OldestFirst
            ? foundEntities.OrderByDescending(AgeOf)
            : data.ChooseRandom
                ? foundEntities.OrderBy(_ => Random.value)
                : foundEntities;
        string[] entities = [.. ordered.Take(maxCount).Select(e => e.GetEntityId().ToString())];
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

    static int AgeOf(BaseComponent entity) => entity.GetComponent<Character>()?.Age ?? int.MinValue;

    IEnumerable<BaseComponent> FindEntities(FindEntitiesData data, SpecChronicleEventController controller)
    {
        var areas = data.AreasBounds;
            
        if (data.CharacterType != CharacterType.Unknown)
        {
            foreach (var c in findEntityHelper.GetCharacters(data.CharacterType, areas))
            {
                yield return c;
            }
        }

        if (data.AllBuildings)
        {
            foreach (var c in findEntityHelper.FindBuildings(areas: areas, areaCondition: data.AreaCondition))
            {
                yield return c;
            }
        }
        else
        {
            var templateNames = controller.FormatTextsRemoveEmpty(data.TemplateNames).ToArray();
            var templatePrefixes = controller.FormatTextsRemoveEmpty(data.TemplatePrefixes).ToArray();

            if (templateNames.Length != 0 || templatePrefixes.Length != 0)
            {
                foreach (var c in findEntityHelper.FindEntitiesByTemplates(templateNames, templatePrefixes, areas, data.AreaCondition))
                {
                    yield return c;
                }
            }
        }
    }
}
