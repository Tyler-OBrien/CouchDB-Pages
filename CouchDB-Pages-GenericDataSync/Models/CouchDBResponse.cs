using System.Text.Json.Serialization;

namespace CouchDBPages.GenericDataSync.Models;

public class CouchDBResponse
{
    [JsonPropertyName("ok")] public bool Ok { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("rev")] public string Rev { get; set; }
}