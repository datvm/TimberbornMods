
namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class BlueprintHandler(ISpecService specs, HttpBlueprintSerializer serializer) : IMoreHttpApiHandler, ILoadableSingleton
{
    readonly SpecService specs = (SpecService)specs;

    public string Endpoint => "blueprints";

    FrozenDictionary<string, Type> specTypeMapping = null!;

    public void Load()
    {
        specTypeMapping = specs._cachedBlueprintsBySpecs.Keys.ToFrozenDictionary(kv => kv.FullName);
    }

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
    {
        return parsedRequestPath.RemainingSegment.Length switch
        {
            0 => await context.HandleAsync(GetSpecNames),
            1 => parsedRequestPath.RemainingSegment[0].ToLowerInvariant() switch
            {
                "specs" => await context.HandleAsync(() => GetSpec(parsedRequestPath)),
                "get" => await context.HandleAsync(() => GetBlueprintAtPath(parsedRequestPath)),
                "get-many" => await context.HandleAsync(() => GetBlueprintsAtPaths(context)),
                "export" => await context.HandleAsync(ExportAsync),
                _ => false,
            },
            _ => false,
        };
    }

    public async Task<ImmutableArray<string>> GetSpecNames() => specTypeMapping.Keys;

    public async Task<JToken> GetBlueprintAtPath(ParsedRequestPath parsedRequestPath)
    {
        var path = parsedRequestPath.QueryParameters.Get("path");
        if (string.IsNullOrEmpty(path))
        {
            throw new StatusCodeException(400, "Missing 'path' query parameter");
        }

        var bp = specs.GetBlueprint(path);
        return serializer.SerializeBlueprint(bp);
    }

    public async Task<JArray> GetBlueprintsAtPaths(HttpListenerContext ctx)
    {
        var body = ctx.Request.InputStream;
        using var reader = new StreamReader(body);
        var paths = (await reader.ReadToEndAsync()).Split(';');
        if (paths.Length == 0)
        {
            throw new StatusCodeException(400, "Missing or empty 'paths' query parameter");
        }

        var blueprints = paths.Select(path =>
        {
            var bp = specs.GetBlueprint(path);
            return serializer.SerializeBlueprint(bp);
        }).ToArray();

        return [.. blueprints];
    }

    public async Task<JArray> GetSpec(ParsedRequestPath parsedRequestPath)
    {
        var typeName = parsedRequestPath.QueryParameters.Get("type");
        if (string.IsNullOrEmpty(typeName))
        {
            throw new StatusCodeException(400, "Missing 'type' query parameter");
        }

        if (!specTypeMapping.TryGetValue(typeName, out var type))
        {
            throw new StatusCodeException(404, $"Spec type '{typeName}' not found");
        }

        var values = specs._cachedBlueprintsBySpecs[type]
            .Select(bp => serializer.SerializeBlueprint(bp.Value))
            .ToArray();

        return [.. values];
    }

    public async Task ExportAsync()
    {
        var folder = PersistenceService.GetExportFolder();

        foreach (var (path, lazy) in specs._cachedBlueprintsByPath)
        {
            var bp = lazy.Value;
            var filePath = Path.Combine(folder, $"{path}.json");

            var folderPath = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(folderPath!);

            var serializableBp = serializer.GetSerializableBlueprint(bp);
            var json = JsonConvert.SerializeObject(serializableBp, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);
        }
    }

}
