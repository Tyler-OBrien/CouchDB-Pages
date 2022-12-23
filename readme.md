# CouchDB-Pages
This project aims to provide similar functionality as Cloudflare or Github Pages, allowing you to host static files, including view previews of pull requests or non-master/main branch commits. Essentially, a versionized file host.

All data is stored in CouchDB as Attachments, so it is best to limit file size to 8 MB or so.


This was designed for use with multiple regions, using CouchDB Replication.

I am running this myself on https://tobrien.dev (https://tobrien.dev/cdn-cgi/trace-json), using a bunch of low-spec rented VPS and IPv4 & IPV6 BGP Anycast (Please excuse poor latency/bad routing. Need to clean up IPV6 routes & only 3 of the VPS's are on IPv4 Anycast).

## CouchDB-Pages-Server
CouchDB Pages is the bulk of the code, an ASP.NET Core application handling serving requests and uploads.

[More info and setup](CouchDB-Pages-Server/readme.md)


## CouchDB-Uploader

[CouchDB-Pages-Uploader](CouchDB-Uploader/readme.md) handles uploading a directory to CouchDB-Pages-Server. It can upload previews (i.e commits not on master/main branch), or new site releases.

It can be used with Github Actions or any CI/CD Environment. 

[More info](CouchDB-Uploader/readme.md)


## CouchDB-Pages-GenericDataSync
[Generic Data Sync](CouchDB-Pages-GenericDataSync/readme.md) uses CouchDB to sync files. 

It can be configured to write/upload files to CouchDB, or download/pull files from CouchDB.

It encrypts/decrypts the files using XChaCha20-Poly1035, using libsodium via the .NET wrapper libsodium-net, and an auto-generated key (which you will have to securely transfer separately).

I made it so I could have at least a bit more security than storing raw private keys/etc in CouchDB.

[More Info](CouchDB-Pages-GenericDataSync/readme.md)

#

[Quick Start](docs/Quick_Start.md)



Todo:

Full coverage with Unit Tests & Clean up (right now, only testing services + main controller for CouchDB)

Integration / Acceptance Tests

Automated builds

Automate the creation of databases and API Key

Prometheus/Grafana monitoring


