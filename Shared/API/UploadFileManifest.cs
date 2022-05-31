namespace CouchDBPages.Shared.API;

public class UploadFileManifest
{
    // Empty Constructor for JSON
    public UploadFileManifest()
    {
    }

    public UploadFileManifest(string gitHash, string hostname, Dictionary<string, string> urlHashDictionary,
        bool preview)
    {
        GitHash = gitHash;
        Hostname = hostname;
        URLHashDictionary = urlHashDictionary;
        Preview = preview;
    }

    public string GitHash { get; set; }

    public string Hostname { get; set; }

    // A Dictionary with each Key being the relative file path (i.e example.com/free-cookies/pineapple.jpg would equal freecookies/pineapple.jpg) and the key being the sha256 hash of the d ata
    public Dictionary<string, string> URLHashDictionary { get; set; }


    public bool Preview { get; set; }
}