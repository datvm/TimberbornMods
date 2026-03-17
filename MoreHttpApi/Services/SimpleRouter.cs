namespace MoreHttpApi.Services;

[BindSingleton]
public class SimpleRouter(IEnumerable<IMoreHttpApiHandler> handlers) : ILoadableSingleton
{

    FrozenDictionary<string, IMoreHttpApiHandler> handlerEndpoints = null!;

    public void Load()
    {
        handlerEndpoints = handlers.ToFrozenDictionary(h => h.Endpoint, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<bool> TryRouteAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
    {
        if (!handlerEndpoints.TryGetValue(parsedRequestPath.RouterSegment, out var handler))
        {
            Debug.LogWarning($"[{nameof(MoreHttpApi)}] No handler found for endpoint: {string.Join('/', [parsedRequestPath.RouterSegment, .. parsedRequestPath.RemainingSegment])}");

            return false;
        }

        TimberUiUtils.LogVerbose(() => $"[{nameof(MoreHttpApi)}] Handled by {handler.GetType().Name}: " +
            string.Join('/', [parsedRequestPath.RouterSegment, ..parsedRequestPath.RemainingSegment]));

        await Awaitable.MainThreadAsync();
        return await handler.HandleAsync(context, parsedRequestPath);        
    }

}
