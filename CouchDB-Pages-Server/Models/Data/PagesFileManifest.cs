using System.Text.Json.Serialization;
using CouchDBPages.Server.Extensions;
using CouchDBPages.Shared.API;

namespace CouchDBPages.Server.Models.Data;

public class PagesFileManifest : IValidator
{
    // Default Constructor for JSON
    public PagesFileManifest()
    {
    }

    public PagesFileManifest(string gitHash, string hostname, Dictionary<string, string> urlHashDictionary,
        bool preview)
    {
        GitHash = gitHash;
        Hostname = hostname;
        URLHashDictionary = urlHashDictionary;
        Preview = preview;
    }

    public PagesFileManifest(UploadFileManifest uploadFileManifest)
    {
        GitHash = uploadFileManifest.GitHash;
        Hostname = uploadFileManifest.Hostname;
        URLHashDictionary = uploadFileManifest.URLHashDictionary;
        Preview = uploadFileManifest.Preview;
    }

    [JsonPropertyName("_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string ID { get; set; }

    [JsonPropertyName("_rev")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Revision { get; set; }


    public string GitHash { get; set; }

    public string Hostname { get; set; }

    // A Dictionary with each Key being the relative file path (i.e example.com/free-cookies/pineapple.jpg would equal freecookies/pineapple.jpg) and the key being the sha256 hash of the d ata
    public Dictionary<string, string> URLHashDictionary { get; set; }


    public bool Preview { get; set; }

    public bool Validate()
    {
        return Extensions.Extensions.Validate(
            (ID.Validate(), nameof(ID)),
            (Revision.Validate(), nameof(Revision)),
            (GitHash.Validate(), nameof(GitHash)),
            (Hostname.Validate(), nameof(Hostname)),
            (URLHashDictionary.Validate(), nameof(URLHashDictionary)),
            (Preview.Validate(), nameof(Preview))
        );
    }
}