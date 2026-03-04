namespace MoreHttpApi.Services;

public readonly record struct ParsedRequestPath(
    string RouterSegment,
    string[] RemainingSegment,
    NameValueCollection QueryParameters
);

[MultiBind(typeof(IHttpApiEndpoint))]
public class MoreHttpApiEndpoint(SimpleRouter simpleRouter) : IHttpApiEndpoint
{

    public async Task<bool> TryHandle(HttpListenerContext context)
    {
        var path = context.Request.Url.AbsolutePath;

        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !string.Equals(parts[0], MoreHttpApiUtils.EndpointStart, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        context.AddCorsHeaders();

        var routerSegment = parts[1];
        var remainingSegment = parts.Skip(2).ToArray();

        var query = System.Web.HttpUtility.ParseQueryString(context.Request.Url.Query);
        await simpleRouter.TryRouteAsync(
            context, 
            new(routerSegment, remainingSegment, query));

        return true;
    }

}
