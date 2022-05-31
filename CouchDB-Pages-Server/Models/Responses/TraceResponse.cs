using Microsoft.AspNetCore.Connections.Features;

namespace CouchDBPages.Server.Models.Responses;

public class TraceResponse
{
    public TraceResponse()
    {
    }

    public TraceResponse(string host, string ip, string scheme, string userAgent, string requestProtocol,
        string tlsProtocol, TraceResponseTLS tls, string location, string machineId, string geographicLocation)
    {
        Host = host;
        IP = ip;
        Scheme = scheme;
        UserAgent = userAgent;
        RequestProtocol = requestProtocol;
        TLSProtocol = tlsProtocol;
        TLS = tls;
        Location = location;
        MachineId = machineId;
        GeographicLocation = geographicLocation;
    }

    public string Host { get; }
    public string IP { get; }

    public string Scheme { get; }

    public string UserAgent { get; }

    public string RequestProtocol { get; }

    public string TLSProtocol { get; }

    public TraceResponseTLS TLS { get; }

    public string Location { get; }

    public string MachineId { get; }

    public string GeographicLocation { get; }
}

public class TraceResponseTLS
{
    public TraceResponseTLS()
    {
    }

    public TraceResponseTLS(string cipherAlgorithm, string keyExchangeAlgorithm, string hashAlgorithm)
    {
        CipherAlgorithm = cipherAlgorithm;
        KeyExchangeAlgorithm = keyExchangeAlgorithm;
        HashAlgorithm = hashAlgorithm;
    }

    public TraceResponseTLS(ITlsHandshakeFeature? tlsHandshakeFeature)
    {
        CipherAlgorithm = tlsHandshakeFeature?.CipherAlgorithm.ToString() ?? "Unknown";
        // https://github.com/dotnet/runtime/issues/55570
        // 44500 is not included in the enum, but means ECDH_Ephem  (Ephemeral elliptic curve Diffie-Hellman key exchange algorithm)
        if ((int)(tlsHandshakeFeature?.KeyExchangeAlgorithm ?? 0) == 44550)
            KeyExchangeAlgorithm = "ECDH_Ephem";
        else
            KeyExchangeAlgorithm = tlsHandshakeFeature?.KeyExchangeAlgorithm.ToString() ?? "Unknown";

        HashAlgorithm = tlsHandshakeFeature?.HashAlgorithm.ToString() ?? "Unknown";
    }

    public string CipherAlgorithm { get; }
    public string KeyExchangeAlgorithm { get; }
    public string HashAlgorithm { get; }
}