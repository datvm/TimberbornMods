namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class LocHandler(ILoc t) : IMoreHttpApiHandler
{
    readonly Loc t = (Loc)t;
    public string Endpoint { get; } = "loc";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => await context.HandleAsync(GetKeys);

    async Task<Dictionary<string, string>> GetKeys() => t._localization;

}
