using System.Text.Json.Serialization;
using CouchDBPages.Server.Extensions;
using CouchDBPages.Server.Models.Data;

namespace CouchDBPages.Server.Models.Responses;

public class CouchResponse : IValidator
{
    [JsonPropertyName("ok")] public bool Ok { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("rev")] public string Rev { get; set; }


    public bool Validate()
    {
        return Extensions.Extensions.Validate(
            (Ok.Validate(), nameof(Ok)),
            (Id.Validate(), nameof(Id)),
            (Rev.Validate(), nameof(Rev))
        );
    }
}