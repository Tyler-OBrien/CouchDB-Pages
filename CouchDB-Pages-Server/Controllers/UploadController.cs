using CouchDBPages.Server.Middleware;
using CouchDBPages.Server.Models.Services;
using CouchDBPages.Shared.API;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CouchDBPages.Server.Controllers;

[Route("api/v1/Upload")]
[ApiController]
[ApiKeyMiddleware]
public class UploadController : ControllerBase
{
    private readonly IFileDataManifestService _fileManifestService;
    private readonly IFileDataService _fileService;

    public UploadController(IFileDataManifestService fileManifestService, IFileDataService fileDataService)
    {
        _fileManifestService = fileManifestService;
        _fileService = fileDataService;
    }


    // POST api/<UploadController>
    [HttpPost("Manifest")]
    public async Task<ActionResult> UploadManifest([FromBody] UploadFileManifest uploadManifest)
    {
        var uploadManifestResult = await _fileManifestService.PutMetadata(uploadManifest);
        // Will return either OK (200) if already exists, or Accepted (202). Accepted is used since this is meant to be used with Couchdb in a cluster, so one node accepting doesn't mean the others will.
        return StatusCode((int)uploadManifestResult.StatusCode);
    }

    // POST api/<UploadController>
    [HttpPost("File")]
    public async Task<ActionResult> UploadFileData([FromBody] UploadFileData file)
    {
        var uploadManifestResult = await _fileService.PutFile(file);
        // Will return either OK (200) if already exists, or Accepted (202). Accepted is used since this is meant to be used with Couchdb in a cluster, so one node accepting doesn't mean the others will.
        return StatusCode((int)uploadManifestResult.StatusCode);
    }

    // POST api/<UploadController>
    [HttpGet("File/{fileHash}")]
    public async Task<ActionResult> GetFile(string fileHash, CancellationToken token)
    {
        var uploadManifestResult = await _fileService.GetFileMetadata(fileHash, token);
        if (uploadManifestResult == null)
            return NotFound();
        return Ok(uploadManifestResult);
    }
}