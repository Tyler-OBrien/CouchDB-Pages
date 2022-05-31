using CouchDBPages.Server.Models.Responses;

namespace CouchDBPages.Server.Models.Services;

public interface IFileService
{
    Task<FileDataResponse?> GetFile(string hostName, string path);
}