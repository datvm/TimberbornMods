namespace MoreHttpApi.Services;

public readonly record struct ParsedRequestPath(
    string RouterSegment,
    string[] RemainingSegment,
    NameValueCollection QueryParameters
);

[MultiBind(typeof(IHttpApiEndpoint))]
public class MoreHttpApiEndpoint(SimpleRouter simpleRouter, MSettings s) : IHttpApiEndpoint
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

        if (context.Request.HttpMethod == "OPTIONS")
        {
            await context.WriteText("OK", 200);
            return true;
        }
       
        var auth = s.Authentication.Value;
        if (!string.IsNullOrEmpty(auth))
        {
            var header = context.Request.Headers["Authorization"];
            if (header != auth)
            {
                await context.WriteText("Unauthorized", 401);
                return true;
            }
        }

        var routerSegment = parts[1];
        var remainingSegment = parts.Skip(2).ToArray();

        var query = System.Web.HttpUtility.ParseQueryString(context.Request.Url.Query);
        var handled = await simpleRouter.TryRouteAsync(
            context, 
            new(routerSegment, remainingSegment, query));
        if (!handled)
        {
            await context.WriteText("Not found", 404);
        }

        return true;
    }

}
