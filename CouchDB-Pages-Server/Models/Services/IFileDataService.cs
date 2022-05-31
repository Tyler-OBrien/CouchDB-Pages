using CouchDBPages.Server.Models.Data;
using CouchDBPages.Server.Models.Responses;
using CouchDBPages.Shared.API;

namespace CouchDBPages.Server.Models.Services;

public interface IFileDataService
{
    Task<PagesFile?> GetFileMetadata(string fileHashValue);

    Task<FileDataResponse?> GetFile(string fileHashValue);


    Task<GenericResponse> PutFile(UploadFileData fileData);
}