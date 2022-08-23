using CouchDBPages.Server.Models.Data;

namespace CouchDBPages.Server.Brokers;

public partial interface IAPIBroker
{
    Task<PagesFile?> FindFileAsync(string id, CancellationToken token);

    Task<HttpResponseMessage> PutFileAsync(PagesFile newFile);

    Task<HttpResponseMessage> GetFileAttachment(string fileHash, CancellationToken token);

    Task<HttpResponseMessage> PutFileAttachmentAsync(string fileName, byte[] file, string contentType, string revision);
}