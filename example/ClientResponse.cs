using System.Net;
using System.Text.Json;

namespace Example;

public class ClientResponse
{
    public HttpStatusCode StatusCode { get; }
    public string Content { get; }

    public ClientResponse(
        HttpStatusCode statusCode,
        string content)
    {
        StatusCode = statusCode;
        Content = content;
    }
}
public class ClientResponse<T> : ClientResponse
{
    public ClientResponse(
        HttpStatusCode statusCode, string content)
        : base(statusCode, content)
    {
    }

    public T Value => JsonSerializer.Deserialize<T>(Content);
}
