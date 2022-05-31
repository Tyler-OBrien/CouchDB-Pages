using System.Text.Json.Serialization;

namespace CouchDBPages.GenericDataSync.Models;

public class PagesSecret
{
    public string Secret { get; set; }

    public string Secret_Name { get; set; }

    public string Owner { get; set; }

    [JsonPropertyName("_id")] public string ID { get; set; }

    [JsonPropertyName("_rev")] public string Revision { get; set; }
}