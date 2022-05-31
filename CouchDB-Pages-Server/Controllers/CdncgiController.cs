using CouchDBPages.Server.Models.Config;
using CouchDBPages.Server.Models.Responses;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Extensions;

namespace CouchDBPages.Server.Controllers;

[ApiController]
[Route("/cdn-cgi")]
[Route("/cdn-cgi/v1/")]
// If hiding behind Cloudflare/another reverse proxy
[Route("/cdn-cgi-alt/v1/")]
public class CdncgiController : ControllerBase
{
    private readonly ApplicationConfig _applicationConfig;

    public CdncgiController(IOptions<ApplicationConfig> applicationConfig)
    {
        _applicationConfig = applicationConfig.Value;
    }

    [HttpGet("trace")]
    public ActionResult GetTrace()
    {
        return Ok(
            $"h={HttpContext.Request.Host.Host}\nip={HttpContext.Connection.LocalIpAddress}\nts={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}\nvisit_scheme={HttpContext.Request.Scheme}\nuag={HttpContext.Request.Headers.UserAgent}\ncolo={_applicationConfig.LocationName}\nhttp={HttpContext.Request.Protocol}\nloc={_applicationConfig.GeographicalLocation}\ntls={HttpContext.Features.Get<ITlsHandshakeFeature>()?.Protocol.GetDisplayName()}");
    }

    [HttpGet("trace-json")]
    public ActionResult<TraceResponse> GetTraceJson()
    {
        var tlsHandshakeFeature = HttpContext.Features.Get<ITlsHandshakeFeature>();
        return new OkObjectResult(new TraceResponse(HttpContext.Request.Host.Host,
            HttpContext.Connection.LocalIpAddress?.ToString() ?? "Unknown", HttpContext.Request.Scheme,
            HttpContext.Request.Headers.UserAgent, HttpContext.Request.Protocol,
            tlsHandshakeFeature?.Protocol.ToString() ?? "None", new TraceResponseTLS(tlsHandshakeFeature),
            _applicationConfig.LocationName, _applicationConfig.MachineName, _applicationConfig.GeographicalLocation));
    }
}