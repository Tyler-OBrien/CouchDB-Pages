using CouchDBPages.Server.Models.Data;

namespace CouchDBPages.Server.Models.Services;

public interface ISecretsService
{
    Task<PagesSecret?> GetSecret(string SecretID);
}