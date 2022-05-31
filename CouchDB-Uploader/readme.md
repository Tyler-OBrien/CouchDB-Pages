# CouchDB-Uploader
Handles uploading files from a local directory


CouchDB Uploader uploads each file one by one from the input directory, including from subdirectories.

For each file, a sha256 hash is generated and we check with the CouchDB Pages Server to see if it is already uploaded.

If not, we encode the file as base64 and upload it to the server.

Once all files are uploaded, we create and upload a Manifest for that specific deployment. The manifest includes a dictionary that links each relative file path (i.e "about-us/index.html") back to the generated sha256 hash.

The deployment may be a "preview" deployment, as it was from a commit to a non-main/master branch. The manifest has a "Preview" bool value signalling this, and the CouchDB Pages Web Server will only upload a preview deployment. 


Usage: ./CouchDB-Pages-Uploader.exe {directory} "{hostName}" "{Git_SHA}" "{Git_Branch}" "{API-KEY}" "{CouchDB-Pages Server URL}"

You can pass all arguments to it like that, but it is intended to be used in Github Actions (or any other CI/CD Pipeline), and just use environment variables.


Github Actions Example:

https://github.com/Tyler-OBrien/Personal-Website-CouchDB-Pages/blob/master/.github/workflows/couchdb_deploy.yml


Environment Variables:

UPLOAD_DIRECTORY - Path of the folder to upload

HOSTNAME - Hostname of the site to upload for (for example, if it was set to tobrien.me, it would upload for that site, and preview domain would be {GIT_SHA}.tobrien.me)

GITHUB_SHA - Commit Sha (used for Preview Domain) - Github Actions default

BRANCH_NAME - Commit Branch (if not master/main, will be a preview deploy, and only deploy to preview domain)

API_KEY - API Key for your CouchDB Pages Server, set with the server install

SERVER_URL - Public URL for your CouchDB Pages Server
