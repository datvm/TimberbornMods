namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class MiscHandler(
    ModRepository modRepository,
    EntityRegistry registry,
    EntitySelectionService selectionService
) : IMoreHttpApiHandler
{
    public string Endpoint { get; } = "misc";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
    {
        return parsedRequestPath.RemainingSegment.Length switch
        {
            0 => await context.HandleAsync(GetHomePageInfoAsync),
            _ => parsedRequestPath.RemainingSegment[0] switch
            {
                "mods" => await context.HandleAsync(GetModsAsync),
                "select" => await context.HandleAsync(() => SelectEntityAsync(parsedRequestPath)),
                "rename" => await context.HandleAsync(() => RenameAsync(parsedRequestPath)),
                _ => false,
            },
        };
    }

    public async Task<HttpHomePageInfo> GetHomePageInfoAsync()
    {
        var gameVersion = GameVersions.CurrentVersion;
        return new(gameVersion.Http(), await GetModsAsync());
    }

    public async Task<HttpMod[]> GetModsAsync() => [.. modRepository.Mods
        .Select(m => {
            var manifest = m.Manifest;
            return new HttpMod(
                manifest.Id, manifest.Name, manifest.Version.Http(),
                m.ModDirectory.Directory.FullName, m.IsEnabled);
        })];

    public async Task SelectEntityAsync(ParsedRequestPath parsedRequestPath)
    {
        if (!Guid.TryParse(parsedRequestPath.RemainingSegment[1], out var id)) { return; }

        var focus = parsedRequestPath.QueryParameters.HasSwitch("focus");
        var follow = parsedRequestPath.QueryParameters.HasSwitch("follow");

        var entity = registry.GetEntity(id);
        if (!entity) { return; }

        if (follow)
        {
            selectionService.SelectAndFollow(entity);
        }
        else if (focus)
        {
            selectionService.SelectAndFocusOn(entity);
        }
        else
        {
            selectionService.Select(entity);
        }
    }

    public async Task RenameAsync(ParsedRequestPath parsedRequestPath)
    {
        if (!Guid.TryParse(parsedRequestPath.RemainingSegment[1], out var id)) { return; }
        
        var newName = parsedRequestPath.QueryParameters.Get("newName");
        if (string.IsNullOrEmpty(newName))
        {
            throw new StatusCodeException(400, "Missing 'newName' query parameter.");
        }
        
        var entity = registry.GetEntity(id);
        if (!entity) { return; }

        var namedEntity = entity.GetComponent<NamedEntity>();
        if (!namedEntity || !namedEntity.IsEditable) { return; }

        namedEntity.SetEntityName(newName);
    }

}
