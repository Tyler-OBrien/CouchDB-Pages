using CouchDBPages.Server.Extensions;
using CouchDBPages.Server.Models.Data;

namespace CouchDBPages.Server.Brokers;

public partial class APIBroker
{
    public readonly string filesDataDB;

    public async Task<HttpResponseMessage> GetFileAttachment(string fileHash, CancellationToken token)
    {
        return await _httpClient.GetAsync("/" + filesDataDB + "/" + Uri.EscapeDataString(fileHash) + "/data");
    }

    public async Task<PagesFile?> FindFileAsync(string id, CancellationToken token)
    {
        return await _httpClient.GetFromJsonAsyncSupportNull<PagesFile>("/" + filesDataDB + "/" +
                                                                        Uri.EscapeDataString(id), token);
    }

    public async Task<HttpResponseMessage> PutFileAsync(PagesFile newFile)
    {
        return await _httpClient.PutAsJsonAsync($"/{filesDataDB}/{Uri.EscapeDataString(newFile.ID)}", newFile);
    }

    public async Task<HttpResponseMessage> PutFileAttachmentAsync(string fileName, byte[] file, string contentType,
        string revision)
    {
        // Create the request message
        var filePut = new HttpRequestMessage(HttpMethod.Put,
            $"/{filesDataDB}/{Uri.EscapeDataString(fileName)}/data?rev={Uri.EscapeDataString(revision)}");

        filePut.Content = new ByteArrayContent(file);


        filePut.Content.Headers.Add("Content-Type", contentType);

        // Upload it!
        return await _httpClient.SendAsync(filePut);
    }
}