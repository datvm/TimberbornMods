namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class DistrictHandler : IMoreHttpApiHandler
{
    public string Endpoint => "districts";

    public Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
    {
        throw new NotImplementedException();
    }
}
