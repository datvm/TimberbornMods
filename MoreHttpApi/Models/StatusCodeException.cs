namespace MoreHttpApi.Models;

public class StatusCodeException(int statusCode, string content) : Exception
{
    public int StatusCode { get; } = statusCode;
    public string Content { get; } = content;
}
