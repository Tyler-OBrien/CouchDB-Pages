# GenericDataSync
"GenericDataSync" is a simple C# Console App designed to push/write data to CouchDB or pull/download data from CouchDB to the local filesystem depending on the configuration. It encrypts the files using XChaCha20-Poly1035, using libsodium via the .NET wrapper libsodium-net. Lets Encrypt has rate limits on certs (5 per hostname per day), so I needed some way to transfer certs to all of the different nodes. 

I made it so I could have at least a bit more security then storing raw private keys/etc in CouchDB.

Files are uploaded to CouchDB using "PagesSecret", the same data model used for CouchDB Server's API Key. The encrypted cipher text is stored as a CouchDB Attachment. The Secret itself is stored with the ID being the file name, and the Secret being the generated nonce.


Configuration Example:

SyncConfig.json
```json
{

  "CouchDB_Username": "user",
  "CouchDB_Password": "<Your password>",
  "CouchDB_URL": "http://localhost:5984",
  "Database": "secrets",
  "RoleTypeInfo": "Reading",
  // If left empty, won't be used. Helpful to know if it breaks!
  "Sentry_DSN": "",
  "FilesToHandle": [
    {
      "FileName": "fullchain.pem",
      "FilePath": "/etc/letsencrypt/live/........./fullchain.pem"
    },
    {
      "FileName": "privkey.pem",
      "FilePath": "/etc/letsencrypt/live/.../privkey.pem"
    }
  ]
}

```