namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class MiscHandler(ModRepository modRepository) : IMoreHttpApiHandler
{
    public string Endpoint { get; } = "misc";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
    {
        return parsedRequestPath.RemainingSegment.Length switch
        {
            0 => await context.HandleAsync(GetHomePageInfoAsync),
            1 => parsedRequestPath.RemainingSegment[0] switch
            {
                "mods" => await context.HandleAsync(GetModsAsync),
                _ => false,
            },
            _ => false,
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

}
