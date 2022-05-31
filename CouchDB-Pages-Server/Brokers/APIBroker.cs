using System.Net.Http.Headers;
using System.Text;
using CouchDBPages.Server.Models.Config;
using Microsoft.Extensions.Options;

namespace CouchDBPages.Server.Brokers;

public partial class APIBroker : IAPIBroker
{
    // These should be cleaned up, right now they are  auto-generated with the name of the data class
    private readonly ApplicationConfig _applicationConfig;
    private readonly HttpClient _httpClient;


    /*
     *        var response = await _fileDataContext.Client.Endpoint.WithBasicAuth(_applicationConfig.CouchDB_Username,
                _applicationConfig.CouchDB_Password)
            .AppendPathSegment("filedatas")
            .AppendPathSegment(Uri.EscapeDataString(fileHashValue))
            .AppendPathSegment(Uri.EscapeDataString("data"))
            .GetAsync();
     */

    public APIBroker(HttpClient httpClient, IOptions<ApplicationConfig> applicationConfig)
    {
        _applicationConfig = applicationConfig.Value;
        _httpClient = httpClient;

        _httpClient.BaseAddress = new Uri(_applicationConfig.CouchDB_URL);

        var authBase64String =
            Convert.ToBase64String(
                Encoding.ASCII.GetBytes(
                    $"{_applicationConfig.CouchDB_Username}:{_applicationConfig.CouchDB_Password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authBase64String);

        filesDataDB = _applicationConfig.CouchDB_Files_Database;
        filesManifestDB = _applicationConfig.CouchDB_Manifest_Database;
        secretsDataDB = _applicationConfig.CouchDB_Secrets_Database;
    }
}