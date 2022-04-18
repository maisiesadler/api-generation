using Microsoft.AspNetCore.Mvc;
using Example.Interactors;
using Example.Models;
using System.Net;

namespace Example;

public class ClientResponse
{
    public ClientResponse(
        HttpStatusCode statusCode,
        string content)
    {

    }
}
public class ClientResponse<T> : ClientResponse
{
    public ClientResponse(
        HttpStatusCode statusCode, string content)
        : base(statusCode, content)
    {
    }
}
