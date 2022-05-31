namespace CouchDBPages.GenericDataSync.Models;

public class FilesToHandle
{
    public string FileName { get; set; }

    public string FilePath { get; set; }

    public bool UnixProtected { get; set; }
}

public class Configuration
{
    public string CouchDB_Username { get; set; }
    public string CouchDB_Password { get; set; }
    public string CouchDB_URL { get; set; }

    public string Database { get; set; }

    public string RoleTypeInfo { get; set; }

    public string Sentry_DSN { get; set; }


    public List<FilesToHandle> FilesToHandle { get; set; }
}