using System.Net;

namespace CouchDBPages.Server.Models.Responses;

public class GenericResponse
{
    public GenericResponse()
    {
    }

    public GenericResponse(HttpStatusCode statusCode, string content)
    {
        StatusCode = statusCode;
        Content = content;
    }

    public HttpStatusCode StatusCode { get; set; }

    public string Content { get; set; }
}