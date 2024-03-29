﻿using System.Text.Json.Serialization;
using CouchDBPages.Server.Extensions;

namespace CouchDBPages.Server.Models.Data;

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