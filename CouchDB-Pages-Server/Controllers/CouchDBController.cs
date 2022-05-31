using CouchDBPages.Server.Middleware;
using CouchDBPages.Server.Models.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace CouchDBPages.Server.Controllers;

public class CouchDBController : ControllerBase
{
    private readonly IFileService _fileService;

    public CouchDBController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpGet]
    [ResponseCacheAttribute(Duration = 60, Location = ResponseCacheLocation.Any)]
    [ETagMiddleware]
    public async Task<ActionResult> GetFromDatabase()
    {
        var hostName = HttpContext.Request.Host.Host;
        var path = HttpContext.Request.Path.Value ?? "";
        // remove slash
        if (path.Length != 0)
            path = path.Substring(1);

        var fileDataResponse = await _fileService.GetFile(hostName, path);

        if (fileDataResponse == null) return NotFound();

        if (EntityTagHeaderValue.TryParse(fileDataResponse.ETag, out var etag))
            return File(fileDataResponse.FileStream, fileDataResponse.ContentType, null, etag);

        return File(fileDataResponse.FileStream, fileDataResponse.ContentType);
    }
}