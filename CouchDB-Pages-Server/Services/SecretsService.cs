using CouchDBPages.Server.Brokers;
using CouchDBPages.Server.Models.Data;
using CouchDBPages.Server.Models.Services;

namespace CouchDBPages.Server.Services;

public class SecretsService : ISecretsService
{
    private readonly IAPIBroker _apiBroker;

    public SecretsService(IAPIBroker apiBroker)
    {
        _apiBroker = apiBroker;
    }

    public async Task<PagesSecret?> GetSecret(string SecretID)
    {
        var secret = await _apiBroker.FindSecretAsync(SecretID);
        return secret;
    }
}