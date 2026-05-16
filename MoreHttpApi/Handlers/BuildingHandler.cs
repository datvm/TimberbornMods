namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class BuildingHandler(
    EntityRegistry entityRegistry,
    ILoc t
) : IMoreHttpApiHandler
{
    public string Endpoint => "buildings";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => parsedRequestPath.RemainingSegment.Length switch
        {
            0 => await context.HandleAsync(ListBuildingsAsync),
            1 => await context.HandleAsync(() => GetBuildingAsync(parsedRequestPath)),
            2 => parsedRequestPath.RemainingSegment[1] switch
            {
                "toggle-pause" => await context.HandleAsync(() => HandlePauseAsync(parsedRequestPath)),
                _ => false,
            },
            _ => false,
        };

    public async Task<HttpGroupedBuildings> ListBuildingsAsync()
    {
        List<HttpBuilding> buildings = [];
        Dictionary<string, HttpLabeledEntity?> specs = [];
        
        foreach (var entity in entityRegistry.Entities)
        {
            if (!entity || !entity.HasComponent<Building>())
            {
                continue;
            }

            var templateName = entity.GetTemplateName();
            if (!specs.ContainsKey(templateName))
            {
                specs[templateName] = entity.GetComponent<LabeledEntity>()?.Http();
            }

            buildings.Add(SerializeBuilding(entity));
        }

        // Group by template name
        Dictionary<string, List<HttpBuilding>> grouped = [];
        foreach (var b in buildings)
        {
            if (!grouped.TryGetValue(b.TemplateName, out var list))
            {
                list = [];
                grouped[b.TemplateName] = list;
            }
            list.Add(b);
        }

        // Create groups sorted by template name
        var groups = grouped
            .OrderBy(g => g.Key)
            .Select(g => new HttpGroupedBuildingTemplate(
                g.Key,
                specs.TryGetValue(g.Key, out var spec) ? spec : null,
                [.. g.Value.OrderBy(b => b.Name.EntityName, StringComparer.OrdinalIgnoreCase)]
            ))
            .ToArray();

        return new(groups);
    }

    public async Task<HttpBuilding> GetBuildingAsync(ParsedRequestPath parsedRequestPath)
    {
        if (!Guid.TryParse(parsedRequestPath.RemainingSegment[0], out var id))
        {
            throw new StatusCodeException(400, "Invalid building ID");
        }

        if (!entityRegistry.TryGetActiveEntity(id, out var entity))
        {
            throw new StatusCodeException(404, "Building not found");
        }

        var templateName = entity.GetTemplateName();
        var templateSpec = entity.GetComponent<LabeledEntity>()?.Http();

        var building = SerializeBuilding(entity);
        building.TemplateSpec = templateSpec;

        return building;
    }

    public async Task HandlePauseAsync(ParsedRequestPath parsedRequestPath)
    {
        if (!Guid.TryParse(parsedRequestPath.RemainingSegment[0], out var id))
        {
            throw new StatusCodeException(400, "Invalid building ID");
        }

        var pausedStr = parsedRequestPath.QueryParameters.Get("paused");
        if (!bool.TryParse(pausedStr, out var paused))
        {
            throw new StatusCodeException(400, "Missing or invalid 'paused' query parameter");
        }

        if (!entityRegistry.TryGetActiveEntity(id, out var entity))
        {
            throw new StatusCodeException(404, "Building not found");
        }

        entity.ActWithComponent<PausableBuilding>(p =>
        {
            if (paused)
            {
                p.Pause();
            }
            else
            {
                p.Resume();
            }
        });
    }

    HttpBuilding SerializeBuilding(EntityComponent entity)
    {
        var named = entity.GetComponent<NamedEntity>();

        HttpBuilding result = new(
            entity.Http(),
            named.Http(),
            entity.GetLabeledName(t),
            entity.GetTemplateName()
        );

        entity.ActWithComponent<PausableBuilding>(p => result.Pausable = new(p.Paused, p.IsPausable()));

        return result;
    }

}