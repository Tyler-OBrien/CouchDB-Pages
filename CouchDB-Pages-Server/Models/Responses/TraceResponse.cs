using Microsoft.AspNetCore.Connections.Features;

namespace CouchDBPages.Server.Models.Responses;

public class TraceResponse
{
    public TraceResponse()
    {
    }

    public TraceResponse(string host, string ip, string scheme, string userAgent, string requestProtocol,
        string tlsProtocol, string tlsCipherInfo, string location, string machineId, string geographicLocation)
    {
        Host = host;
        IP = ip;
        Scheme = scheme;
        UserAgent = userAgent;
        RequestProtocol = requestProtocol;
        TLSProtocol = tlsProtocol;
        TLSCipherInfo = tlsCipherInfo;
        Location = location;
        MachineId = machineId;
        GeographicLocation = geographicLocation;
    }

    public string Host { set; get; }
    public string IP { set; get; }

    public string Scheme { set; get; }

    public string UserAgent { set; get; }

    public string RequestProtocol { set; get; }

    public string TLSProtocol { set; get; }

    public string TLSCipherInfo { set; get; }

    public string Location { set; get; }

    public string MachineId { set; get; }

    public string GeographicLocation { set; get; }
}

