using CouchDBPages.Server.Extensions;
using CouchDBPages.Server.Models.Data;

namespace CouchDBPages.Server.Brokers;

public partial class APIBroker
{
    public readonly string secretsDataDB;

    public async Task<PagesSecret?> FindSecretAsync(string id, CancellationToken token)
    {
        return await _httpClient.GetFromJsonAsyncSupportNull<PagesSecret>(
            $"/{secretsDataDB}/{Uri.EscapeDataString(id)}", token);
    }
}