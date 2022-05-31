namespace CouchDBPages.Server.Models.Responses;

public class FileDataResponse
{
    public FileDataResponse()
    {
    }

    public FileDataResponse(Stream fileStream, string contentType, string contentLength, string eTag,
        string xCouchDbBodyTime)
    {
        FileStream = fileStream;
        ContentType = contentType;
        ContentLength = contentLength;
        ETag = eTag;
        X_CouchDB_Body_Time = xCouchDbBodyTime;
    }

    public Stream FileStream { get; set; }

    public string ContentType { get; set; }

    public string ContentLength { get; set; }


    public string ETag { get; set; }

    public string X_CouchDB_Body_Time { get; set; }
}