namespace MoreHttpApi.Handlers;

public interface IMoreHttpApiHandler
{
    string Endpoint { get; }
    Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath);
}
