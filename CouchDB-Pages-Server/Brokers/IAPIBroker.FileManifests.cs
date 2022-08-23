using CouchDBPages.Server.Models.Data;

namespace CouchDBPages.Server.Brokers;

public partial interface IAPIBroker
{
    Task<PagesFileManifest?> FindManifestAsync(string id, CancellationToken token);


    Task<HttpResponseMessage> PutManifestAsync(PagesFileManifest manifest);
}