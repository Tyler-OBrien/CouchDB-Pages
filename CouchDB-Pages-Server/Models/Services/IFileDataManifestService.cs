using CouchDBPages.Server.Models.Data;
using CouchDBPages.Server.Models.Responses;
using CouchDBPages.Shared.API;

namespace CouchDBPages.Server.Models.Services;

public interface IFileDataManifestService
{
    Task<PagesFileManifest?> GetMetadata(string hostName);


    Task<GenericResponse> PutMetadata(UploadFileManifest newFileManifest);
}