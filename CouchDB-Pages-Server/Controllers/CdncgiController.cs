using System.Text;
using CouchDBPages.Server.Extensions;
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
        var getTraceResponseDetails = this.getTraceResponseDetails(HttpContext);
        var responseStringBuilder = new StringBuilder();
        responseStringBuilder.AppendLine($"h={getTraceResponseDetails.Host}");
        responseStringBuilder.AppendLine($"ip={getTraceResponseDetails.IP}");
        responseStringBuilder.AppendLine($"ts={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
        responseStringBuilder.AppendLine($"visit_scheme={getTraceResponseDetails.Scheme}");
        responseStringBuilder.AppendLine($"ug={getTraceResponseDetails.UserAgent}");
        responseStringBuilder.AppendLine($"colo={getTraceResponseDetails.Location}");
        responseStringBuilder.AppendLine($"http={getTraceResponseDetails.RequestProtocol}");
        responseStringBuilder.AppendLine($"loc={getTraceResponseDetails.GeographicLocation}");
        responseStringBuilder.AppendLine($"tls={getTraceResponseDetails.TLSProtocol}");
        return Ok(responseStringBuilder.ToString());
    }

    [HttpGet("trace-json")]
    public ActionResult<TraceResponse> GetTraceJson()
    {
        return getTraceResponseDetails(HttpContext);
    }

    public TraceResponse getTraceResponseDetails(HttpContext HttpContext)
    {
        var tlsHandshakeFeature = HttpContext.Features.Get<ITlsHandshakeFeature>();
        var newTraceResponse = new TraceResponse();
        newTraceResponse.Host = HttpContext.Request.Host.Host;
        newTraceResponse.IP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        newTraceResponse.Scheme = HttpContext.Request.Scheme;
        newTraceResponse.UserAgent = HttpContext.Request.Headers.UserAgent;
        newTraceResponse.RequestProtocol = HttpContext.Request.Protocol;
        newTraceResponse.TLSProtocol = tlsHandshakeFeature?.Protocol.ToString() ?? "None";
        newTraceResponse.TLSCipherInfo = tlsHandshakeFeature.PrettyPrintCiphersInfo();
        newTraceResponse.Location = _applicationConfig.LocationName;
        newTraceResponse.MachineId = _applicationConfig.MachineName;
        newTraceResponse.GeographicLocation = _applicationConfig.GeographicalLocation;
        if (_applicationConfig.Behind_Reverse_Proxy)
        {
            newTraceResponse.RequestProtocol =
                HttpContext.Request.Headers.TryResolveHeader("X-Forwarded-Proto-Version");
            newTraceResponse.TLSProtocol = HttpContext.Request.Headers.TryResolveHeader("X-Forwarded-TLS-Protocol");
            newTraceResponse.TLSCipherInfo = HttpContext.Request.Headers.TryResolveHeader("X-Forwarded-TLS-Cipher");
        }

        return newTraceResponse;
    }

}