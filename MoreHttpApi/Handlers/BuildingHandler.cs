namespace MoreHttpApi.Handlers;

public class BuildingHandler(
    EntityRegistry entityRegistry,
    ILoc t,
    BuildingSettingsResolver buildingSettingsResolver
) : IMoreHttpApiHandler
{
    public string Endpoint => "buildings";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => parsedRequestPath.RemainingSegment.Length switch
        {
            0 => await context.HandleAsync(ListBuildingsAsync),
            1 => parsedRequestPath.RemainingSegment[0] switch
            {
                "query" => await context.HandleAsync(() => HandleQueryAsync(parsedRequestPath)),
                _ => await context.HandleAsync(() => GetBuildingAsync(parsedRequestPath)),
            },
            2 => parsedRequestPath.RemainingSegment[1] switch
            {
                "toggle-pause" => await context.HandleAsync(() => HandlePauseAsync(parsedRequestPath)),
                _ => false,
            },
            _ => false,
        };

    public async Task<HttpGroupedBuildings> ListBuildingsAsync()
    {
        var groups = BuildGroups(entityRegistry.Entities.Where(e => e && e.HasComponent<Building>()));
        return new(groups);
    }

    public HttpBuildingsResult GetBuildings(IEnumerable<Guid> ids)
    {
        var idSet = ids.ToHashSet();
        var entities = entityRegistry.Entities
            .Where(e => e && e.HasComponent<Building>() && idSet.Contains(e.EntityId));

        var buildings = entities.Select(SerializeBuilding).ToDictionary(b => b.Entity.EntityId);
        var groups = BuildGroups(buildings.Values);

        return new(buildings, groups);
    }

    async Task<HttpBuildingsResult> HandleQueryAsync(ParsedRequestPath parsedRequestPath)
    {
        var idsParam = parsedRequestPath.QueryParameters.Get("ids");
        if (string.IsNullOrEmpty(idsParam))
        {
            throw new StatusCodeException(400, "Missing 'ids' query parameter");
        }

        var ids = idsParam.Split(';')
            .Select(s => Guid.TryParse(s.Trim(), out var g) ? (Guid?)g : null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value);

        return GetBuildings(ids);
    }

    HttpGroupedBuildingTemplate[] BuildGroups(IEnumerable<HttpBuilding> buildings)
    {
        Dictionary<string, (HttpLabeledEntity? Spec, List<HttpBuilding> List)> grouped = [];

        foreach (var b in buildings)
        {
            if (!grouped.TryGetValue(b.TemplateName, out var entry))
            {
                entry = (null, []);
                grouped[b.TemplateName] = entry;
            }
            entry.List.Add(b);
        }

        return [.. grouped
            .OrderBy(g => g.Key)
            .Select(g => new HttpGroupedBuildingTemplate(
                g.Key,
                g.Value.Spec,
                [.. g.Value.List.OrderBy(b => b.Name.EntityName, StringComparer.OrdinalIgnoreCase)]
            ))];
    }

    HttpGroupedBuildingTemplate[] BuildGroups(IEnumerable<EntityComponent> entities)
    {
        List<HttpBuilding> buildings = [];
        Dictionary<string, HttpLabeledEntity?> specs = [];

        foreach (var entity in entities)
        {
            var templateName = entity.GetTemplateName();
            if (!specs.ContainsKey(templateName))
            {
                specs[templateName] = entity.GetComponent<LabeledEntity>()?.Http();
            }
            buildings.Add(SerializeBuilding(entity));
        }

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

        return [.. grouped
            .OrderBy(g => g.Key)
            .Select(g => new HttpGroupedBuildingTemplate(
                g.Key,
                specs.TryGetValue(g.Key, out var spec) ? spec : null,
                [.. g.Value.OrderBy(b => b.Name.EntityName, StringComparer.OrdinalIgnoreCase)]
            ))];
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

        var settingsPairs = buildingSettingsResolver.Get(entity);
        var settingsDict = settingsPairs.ToDictionary(p => p.Settings.Type.FullName, p => p.Settings.Serialize(p.Duplicable));

        HttpBuilding result = new(
            entity.Http(),
            named.Http(),
            entity.GetLabeledName(t),
            entity.GetTemplateName(),
            settingsDict
        );

        entity.ActWithComponent<PausableBuilding>(p => result.Pausable = new(p.Paused, p.IsPausable()));

        return result;
    }

}