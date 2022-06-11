Nginx Info:

As far as I understand, Microsoft has changed their recommendation on always using Kestrel behind a Reverse Proxy. However, using a reverse proxy can still offer other advantages and is easier to "harden" and configure.

If using CouchDB-Pages behind Nginx, set the "Behind_Reverse_Proxy" bool in appsettings.json to True.

For CDN-CGI/trace, it will also depend on a few headers being forwarded to it. In Nginx, you can set these like so (wherever your proxy pass directive is configured)

```nginx
proxy_set_header X-Forwarded-For   $remote_addr;
proxy_set_header X-Forwarded-Proto $scheme;
proxy_set_header X-Forwarded-Proto-Version $server_protocol;
proxy_set_header X-Forwarded-TLS-Protocol $ssl_protocol;
proxy_set_header X-Forwarded-TLS-Cipher $ssl_cipher;
``` 