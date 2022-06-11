Quick Start:

This was designed to work well with a few Ansible Playbooks.

https://github.com/Tyler-OBrien/wireguard-mesh-network-ansible

I created a WireGuard mesh network between all my different nodes, as they were from multiple different providers and I needed some secure communication method between them.

https://github.com/Tyler-OBrien/automatic_couchdb_replication_ansible

To automatically set up replication between all of your nodes, this script can be helpful.

CouchDB Pages expects three different databases already created, by default they are "files", "manifests", and "secrets". You can change them in the appsettings.json

The above script can be used to automatically create and replicate those databases:

ex.
```bash
ansible-playbook -i "inventory.yml" "create_and_replicate_any_db.yml"  -e "database_name=files"
ansible-playbook -i "inventory.yml" "create_and_replicate_any_db.yml"  -e "database_name=manifests"
ansible-playbook -i "inventory.yml" "create_and_replicate_any_db.yml"  -e "database_name=secrets"
```
You also need to manually create your API Secret Document.

You can do this using the Fauxton Web UI for CouchDB (i.e navigate to your Couch DB Server URL /_utils: http://localhost:5984/_utils), and navigating to the secrets database, and then create a document with the following properties:
```json
{
  "_id": "ApiKey",
  "Secret": "<Random value to be used with API Key>",
  "Secret_Name": "API KEY",
  "Owner": "<Any info to keep track of API Key>"
}
```
The CouchDB Pages Server is configured via the appsettings.json file, there is a sample included and auto-generated. [More info about it's configuration](../CouchDB-Pages-Server/readme.md).

I used CouchDB-Pages-GenericDataSync to sync my certs between all different nodes, just aiming for something better than plain text storing. 

Generic Data Sync has its configuration it generates at run time, with two Role types "Writing", which will create a key and save it locally  (synckey.key) and upload to CouchDB, and "Reading" which will read down from CouchDB. 

Unix Protected Setting will make the files only readable/writable by root.

If you are using a distro that uses systemd, [here's a unit file you can use to run it as a service easily](systemd-setup.md).


If you intend to use CouchDB-Pages behind Nginx, [here's some config info](nginx.md). There's some configuration changes and headers it expects.
