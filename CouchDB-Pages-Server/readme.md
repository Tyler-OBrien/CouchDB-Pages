# CouchDB-Pages-Server
CouchDB-Pages-Server is the ASP.NET Web server application, which also handles new deployments from CouchDB-Uploader.

CouchDB Pages Server's main role is just exposing CouchDB. 

Data stored is split up into three main types.

PagesFile "File" which contain metadata and the couchdb attachment for each raw file. Files are accessed and stored by their ID, which is a sha256 hash of their contents, done by the uploader.

PagesFileManifest "Manifest" which contains the details about each deployment. One is created for any preview deployment, which are hosted at {GITHUB_SHA}.hostname, and another for the live website. Manifests contain a dictionary that links the paths of each file to the file content's sha256 hash, which is used to get the file. Essentially, one manifest is stored per hostname.

PagesSecret "secret" are used by GenericDataSync to store nonces for uploaded content, and used in CouchDB Pages Server to store the API Key.

Each request looks up the manifest for that hostname, checks if the requested file is within the manifest, and then will attempt to stream the CouchDB attachment from the CouchDB Server back to the requester.





It uses appsettings.json to store its configuration.

Sample Config:
(Warning: This binds to all IPv4 and Ipv6 IPs using Kestrel)
```json
{
  "AllowedHosts": "*",
  "Application": {
    "CouchDB_Username": "admin",
    "CouchDB_Password": "password",
    "CouchDB_URL": "http://localhost:5984",
    // Optional, if left blank will not be used
    "SENTRY_DSN": "",
    // These values are returned by CDN-CGI/trace or cdn-cgi/trace-json. Below are just suggested formats, but you can make them whatever you want.
    "LocationName": "<IATA AIRPORT, or whatever you want>",
    "GeographicalLocation": "US, NY, etc",
    "MachineName": "<ProviderName> <Location> <Number or ID>",
    // Database names
    "CouchDB_Files_Database": "files",
    "CouchDB_Manifest_Database": "manifests",
    "CouchDB_Secrets_Database": "secrets",
    // Sets the port Prometheus Metrics can be fetched from (at /metrics endpoint). If 0 or default, will not be enabled. Keep in mind if you enable this, you will need to also bind the port in Kestrel.
    "Prometheus_Metrics_Port": 9787,
    // If Behind a proxy, will instruct ASP.NET Core to use at X-Forwarded-For and X-Forwarded-Proto Headers, also assumes some headers exist (See docs/nginx.md)
    "Behind_Reverse_Proxy": false
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://*:80",
        "Protocols": "Http1AndHttp2"
      },
      "HttpPrometheus": {
        "Url": "http://localhost:9787",
        "Protocols": "Http1AndHttp2"
      },
      "Https": {
        "Url": "https://*:443",
        "Protocols": "Http1AndHttp2",
        "Certificate": {
          "Path": "/etc/letsencrypt/live/....../fullchain.pem",
          "KeyPath": "/etc/letsencrypt/live/....../privkey.pem"
        }
      },
    }
  }
}
```