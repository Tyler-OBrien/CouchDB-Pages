using System.Net;
using System.Text.Json;
using CouchDBPages.Server.Brokers;
using CouchDBPages.Server.Extensions;
using CouchDBPages.Server.Models.Data;
using CouchDBPages.Server.Models.Responses;
using CouchDBPages.Server.Models.Services;
using CouchDBPages.Shared.API;
using Microsoft.AspNetCore.StaticFiles;

namespace CouchDBPages.Server.Services;

public class FileDataService : IFileDataService
{
    private readonly IAPIBroker _apiBroker;


    public FileDataService(
        IAPIBroker apiBroker)
    {
        _apiBroker = apiBroker;
    }

    public Task<PagesFile?> GetFileMetadata(string fileHashValue, CancellationToken token)
    {
        return _apiBroker.FindFileAsync(fileHashValue, token);
    }

    public async Task<FileDataResponse?> GetFile(string fileHashValue, CancellationToken token)
    {
        var response = await _apiBroker.GetFileAttachment(fileHashValue, token);

        // Prevent leaking up bad responses
        if (response.StatusCode != HttpStatusCode.OK) return null;


        var contentType = response.Content.Headers.ContentType?.ToString();
        var contentLength = response.Content.Headers.ContentLength.ToString();
        var etag = response.Headers.ETag?.ToString();
        var couchDbBodyTime = response.Headers.TryResolveHeader("X-CouchDB-Body-Time");


        return new FileDataResponse(await response.Content.ReadAsStreamAsync(token), contentType ?? "text/html",
            contentLength ?? "0", etag ?? "", couchDbBodyTime);
    }


    public async Task<GenericResponse> PutFile(UploadFileData file)
    {
        var fileByteArray = Convert.FromBase64String(file.Base64EncodedFile);

        if (new FileExtensionContentTypeProvider().TryGetContentType(file.FileName, out var contentType) == false)
            contentType = "application/octet-stream";


        var newPagesfile = new PagesFile { ID = file.Hash };


        var tryGetFile = await _apiBroker.PutFileAsync(newPagesfile);
        if (tryGetFile.StatusCode == HttpStatusCode.Conflict)
            return new GenericResponse(HttpStatusCode.OK, "File Already uploaded");


        tryGetFile.EnsureSuccessStatusCode();

        var CouchResponse = JsonSerializer.Deserialize<CouchResponse>(await tryGetFile.Content.ReadAsStringAsync());

        if (CouchResponse == null || CouchResponse.Validate() == false)
            return new GenericResponse(HttpStatusCode.InternalServerError, "Error getting file details");


        var response =
            await _apiBroker.PutFileAttachmentAsync(file.Hash, fileByteArray, contentType, CouchResponse.Rev);


        if (response == null || !response.IsSuccessStatusCode)
        {
            if (response == null) return new GenericResponse(HttpStatusCode.InternalServerError, "Unexpected error");
            return new GenericResponse(response.StatusCode, $"{await response.Content.ReadAsStringAsync()}");
        }


        return new GenericResponse(HttpStatusCode.Accepted, "Should replicate shortly...");
    }
}