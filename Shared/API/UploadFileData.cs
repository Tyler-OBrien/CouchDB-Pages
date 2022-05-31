namespace CouchDBPages.Shared.API;

public class UploadFileData
{
    // Empty Constructor for Json
    public UploadFileData()
    {
    }

    public UploadFileData(string hash, string fileName, string base64EncodedFile)
    {
        Hash = hash;
        Base64EncodedFile = base64EncodedFile;
        FileName = fileName;
    }

    public string Hash { get; set; }

    public string FileName { get; set; }

    public string Base64EncodedFile { get; set; }
}