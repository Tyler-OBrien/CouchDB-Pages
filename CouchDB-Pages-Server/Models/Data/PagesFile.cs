using System.Text.Json.Serialization;
using CouchDBPages.Server.Extensions;
using Newtonsoft.Json;

namespace CouchDBPages.Server.Models.Data;

[JsonObject(ItemRequired = Required.Always)]
public class PagesFile : IValidator
{
    [JsonPropertyName("_id")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string ID { get; set; }

    [JsonPropertyName("_rev")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Revision { get; set; }

    public bool Validate()
    {
        return Extensions.Extensions.Validate(
            (ID.Validate(), nameof(ID)),
            (Revision.Validate(), nameof(Revision))
        );
    }
}