Extended Setup Info:


Run as a service (Systemd unit file, assumes you installed the server at /opt/couchdb-pages)
```bash
cat > /etc/systemd/system/couchdb-pages.service<< EOF
[Unit]
Description=Runs CouchDB Pages

[Service]
User=root
WorkingDirectory=/opt/couchdb-pages/
ExecStart=/opt/couchdb-pages/CouchDB-Pages-Server
Restart=always

[Install]
WantedBy=multi-user.target
EOF
```

Startup, make sure is executable, etc
```bash
sudo chmod +x /opt/couchdb-pages/CouchDB-Pages-Server
sudo systemctl enable couchdb-pages
sudo systemctl start couchdb-pages
```
