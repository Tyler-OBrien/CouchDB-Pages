namespace CouchDBPages.Server.Models.Config;

public class ApplicationConfig
{
    public const string Section = "Application";
    public string CouchDB_Username { get; set; }
    public string CouchDB_Password { get; set; }
    public string CouchDB_URL { get; set; }
    public string SENTRY_DSN { get; set; }

    public int Prometheus_Metrics_Port { get; set; }

    public string LocationName { get; set; }

    public string GeographicalLocation { get; set; }

    public string MachineName { get; set; }

    public string CouchDB_Files_Database { get; set; }

    public string CouchDB_Manifest_Database { get; set; }

    public string CouchDB_Secrets_Database { get; set; }
}