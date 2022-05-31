using CouchDBPages.Server.Models.Data;

namespace CouchDBPages.Server.Brokers;

public partial interface IAPIBroker
{
    Task<PagesSecret?> FindSecretAsync(string id);
}