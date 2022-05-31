using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CouchDBPages.GenericDataSync.Extensions;
using CouchDBPages.GenericDataSync.Helpers;
using CouchDBPages.GenericDataSync.Models;
using Mono.Unix;
using Mono.Unix.Native;
using Sentry;
using Serilog;
using Sodium;

/*
 * Made to pull down (or write) files using CouchDB's Attachment feature, and a generic "PagesSecret" document, borrowed from CouchDB-Pages
 * libsodium via libsodium-core is used for XChaCha20-Poly1305 encryption of secrets. 
 */

var client = new HttpClient();


var logger = LogHandler.Init();

var configuration = SimpleConfigurationHandler.SetupOrReadConfig();
var secretsDB = configuration.Database;


using (SentrySdk.Init(o =>
       {
           o.Dsn = string.IsNullOrWhiteSpace(configuration.Sentry_DSN) ? null : configuration.Sentry_DSN;
           o.AttachStacktrace = true;
       }))
{
    client.BaseAddress = new Uri(configuration.CouchDB_URL);


    var authBase64String =
        Convert.ToBase64String(
            Encoding.ASCII.GetBytes(
                $"{configuration.CouchDB_Username}:{configuration.CouchDB_Password}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authBase64String);

    if (configuration.RoleTypeInfo.Equals("Reading", StringComparison.OrdinalIgnoreCase))
    {
        var key = await GetKey(false);


        foreach (var file in configuration.FilesToHandle)
            await TryPullDownAndPutFile(file.FileName, file.FilePath, file.UnixProtected, key);
        Log.Logger.Information($"Finished Reading {configuration.FilesToHandle.Count} files from {secretsDB}.");
    }
    else if (configuration.RoleTypeInfo.Equals("Writing", StringComparison.OrdinalIgnoreCase))
    {
        var key = await GetKey(true);


        foreach (var file in configuration.FilesToHandle)
            await TryUploadFileAsSecret(file.FileName, file.FilePath, key);
        Log.Logger.Information($"Finished Writing {configuration.FilesToHandle.Count} files to {secretsDB}.");
    }

    Log.Logger.Information("Shutting down...");
// Flush all remaining logs to file
    logger.Dispose();
}

async Task TryPullDownAndPutFile(string fileName, string filePath, bool unixProtected, byte[] key)
{
    var tryGetSecret =
        await client.GetFromJsonAsyncSupportNull<PagesSecret>($"/{secretsDB}/{Uri.EscapeDataString(fileName)}");

    if (tryGetSecret == null) throw new Exception("Cannot get file metadata");

    var tryGetFile =
        await client.GetAsync("/" + secretsDB + "/" + Uri.EscapeDataString(fileName) + "/data");

    var GetFileContents = await tryGetFile.Content.ReadAsByteArrayAsync();

    var plaintext =
        SecretAeadXChaCha20Poly1305.Decrypt(GetFileContents, Convert.FromBase64String(tryGetSecret.Secret), key);


    await WriteFile(filePath, plaintext, unixProtected);


    Log.Logger.Information($"Successfully downloaded {fileName} into {filePath}");
}


async Task TryUploadFileAsSecret(string fileName, string filePath, byte[] key)
{
    // Grab the file
    var fileAllBytesAsync = await File.ReadAllBytesAsync(filePath);

    var nonce = new byte[24];
    RandomNumberGenerator.Create().GetBytes(nonce);

    var ciphertext = SecretAeadXChaCha20Poly1305.Encrypt(fileAllBytesAsync, nonce, key);


    // First we try to get the secret (if it exists)
    var tryGetSecret =
        await client.GetFromJsonAsyncSupportNull<PagesSecret>($"/{secretsDB}/{Uri.EscapeDataString(fileName)}");

    var pemSecret = tryGetSecret ?? new PagesSecret();


    pemSecret.Owner = $"Generic Datasync - {DateTime.UtcNow:R}";
    pemSecret.Secret_Name = fileName;
    pemSecret.Secret = Convert.ToBase64String(nonce);

    // Either Update or create anew, not writing null _id or _rev
    var tryPutSecretMetadata = await client.PutAsJsonAsync($"/{secretsDB}/{Uri.EscapeDataString(fileName)}",
        pemSecret, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });

    if (tryPutSecretMetadata.IsSuccessStatusCode == false)
        throw new Exception(
            $"Failure creating {fileName} doc, more details: {await tryPutSecretMetadata.Content.ReadAsStringAsync()}");

    // Now we have to get the new revision number from the response
    var getNewDoc = await tryPutSecretMetadata.Content.ReadFromJsonAsync<CouchDBResponse>();

    if (getNewDoc == null) throw new Exception("Response Doc should not be null");


    // Create the request message
    var filePut = new HttpRequestMessage(HttpMethod.Put,
        $"/{secretsDB}/{Uri.EscapeDataString(fileName)}/data/?rev={getNewDoc.Rev}");

    filePut.Content = new ByteArrayContent(ciphertext);


    // Upload it!
    var tryPutFile = await client.SendAsync(filePut);

    if (tryPutFile.IsSuccessStatusCode == false)
        throw new Exception(
            $"Failure uploading {fileName}, got status code {tryPutFile.StatusCode}, more details: {await filePut.Content.ReadAsStringAsync()}");
    Log.Logger.Information($"Successfully uploaded {fileName} from {filePath}  to {secretsDB}.");
}

const string KEYNAME = "synckey.key";

async Task<byte[]> GetKey(bool shouldGenerate)
{
    if (File.Exists(KEYNAME))
    {
        using var fileStream = new StreamContent(
            new FileStream(KEYNAME, FileMode.Open));

        return await fileStream.ReadAsByteArrayAsync();
    }

    if (shouldGenerate == false)
        throw new Exception("Missing Key for Encryption. The configured Writer should have generated one.");

    // Gen 256-bit key
    var key = new byte[32];
    RandomNumberGenerator.Create().GetBytes(key);

    // Write Key
    await WriteFile(KEYNAME, key, true);
    return key;
}


async Task WriteFile(string filePath, byte[] file, bool unixProtected)
{
    var getDirectory = Path.GetDirectoryName(filePath);

    if (string.IsNullOrEmpty(getDirectory) == false)
        Directory.CreateDirectory(getDirectory);

    // Only tested Linux, but should work fine on FreeBSD or OSX.
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) ||
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        var unixFileInfo = new UnixFileInfo(filePath);

        // User Read & Write, Group Read, Other Read
        var filePerms = FilePermissions.S_IWUSR | FilePermissions.S_IRUSR | FilePermissions.S_IRGRP |
                        FilePermissions.S_IROTH;
        if (unixProtected)
            // Only user read/write
            filePerms = FilePermissions.S_IWUSR | FilePermissions.S_IRUSR;


        var writeStream = unixFileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, filePerms);

        await writeStream.WriteAsync(file);
    }
    else // Windows, etc
    {
        await using var fileStream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite);


        await fileStream.WriteAsync(file);
    }
}