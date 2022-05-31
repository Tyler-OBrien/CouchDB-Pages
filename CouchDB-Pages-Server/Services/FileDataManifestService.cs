using System.Net;
using CouchDBPages.Server.Brokers;
using CouchDBPages.Server.Extensions;
using CouchDBPages.Server.Models.Data;
using CouchDBPages.Server.Models.Responses;
using CouchDBPages.Server.Models.Services;
using CouchDBPages.Shared.API;
using Microsoft.Extensions.Caching.Memory;

namespace CouchDBPages.Server.Services;

public class FileDataManifestService : IFileDataManifestService
{
    private readonly IAPIBroker _apiBroker;
    private readonly IMemoryCache _cache;


    public FileDataManifestService(IMemoryCache cache, IAPIBroker apiBroker)
    {
        _cache = cache;
        _apiBroker = apiBroker;
    }

    public async Task<PagesFileManifest?> GetMetadata(string hostName)
    {
        if (_cache.TryGetValue(hostName, out PagesFileManifest? manifest)) return manifest;

        manifest = await _apiBroker.FindManifestAsync(hostName);

        if (manifest != null)
        {
            using var entry = _cache.CreateEntry(hostName);
            entry.SetValue(manifest);
            entry.SetAbsoluteExpiration(TimeSpan.FromSeconds(30));
            // Just assuming each one is 8 kb
            entry.SetSize(8000);
        }

        return manifest;
    }


    public async Task<GenericResponse> PutMetadata(UploadFileManifest uploadManifest)
    {
        var fileManifest = new PagesFileManifest(uploadManifest) { ID = uploadManifest.Hostname };

        fileManifest.ID = $"{fileManifest.GitHash}.{fileManifest.Hostname}";
        fileManifest.Hostname = $"{fileManifest.GitHash}.{fileManifest.Hostname}";

        var result = await _apiBroker.PutManifestAsync(fileManifest);

        if (result.StatusCode == HttpStatusCode.Conflict)
            return new GenericResponse(HttpStatusCode.Conflict, "This commit was already uploaded..");

        await result.VerifyCouchOk();


        if (!uploadManifest.Preview)
        {
            var tryGetCurrentManifest =
                await _apiBroker.FindManifestAsync(uploadManifest.Hostname);
            if (tryGetCurrentManifest == null || string.IsNullOrWhiteSpace(tryGetCurrentManifest.ID))
            {
                var indexManifest = new PagesFileManifest(uploadManifest) { ID = uploadManifest.Hostname };


                await (await _apiBroker.PutManifestAsync(indexManifest)).VerifyCouchOk();
            }
            else
            {
                tryGetCurrentManifest.Hostname = uploadManifest.Hostname;
                tryGetCurrentManifest.GitHash = uploadManifest.GitHash;
                tryGetCurrentManifest.Preview = uploadManifest.Preview;
                tryGetCurrentManifest.URLHashDictionary = uploadManifest.URLHashDictionary;
                await (await _apiBroker.PutManifestAsync(tryGetCurrentManifest)).VerifyCouchOk();
            }
        }

        return new GenericResponse(HttpStatusCode.Accepted, "Nom");
    }
}