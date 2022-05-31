using System.Text.Json.Serialization;
using CouchDBPages.Server.Extensions;

namespace CouchDBPages.Server.Models.Data;

public class PagesSecret : IValidator
{
    [JsonPropertyName("_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string ID { get; set; }

    [JsonPropertyName("_rev")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Revision { get; set; }

    public string Secret { get; set; }

    public string Secret_Name { get; set; }

    public string Owner { get; set; }

    public bool Validate()
    {
        return Extensions.Extensions.Validate(
            (ID.Validate(), nameof(ID)),
            (Revision.Validate(), nameof(Revision)),
            (Secret.Validate(), nameof(Secret)),
            (Secret_Name.Validate(), nameof(Secret_Name)),
            (Owner.Validate(), nameof(Owner))
        );
    }
}