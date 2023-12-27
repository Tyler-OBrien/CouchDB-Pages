using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using CouchDBPages.Shared.API;
using ShellProgressBar;

const string UploadManifestEndpoint = "/api/v1/Upload/Manifest";
const string UploadFileEndpoint = "/api/v1/Upload/File";


using var client = new HttpClient();
client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;

Console.WriteLine("Loading...");


var filePath = GetFromArgsOrEnv("File Path", 0, "UPLOAD_DIRECTORY");

if (filePath == null) return 1;


var hostName = GetFromArgsOrEnv("Host Name", 1, "HOSTNAME");

if (hostName == null) return 1;


var gitHash = GetFromArgsOrEnv("Github Commit Sha", 2, "GITHUB_SHA");

if (gitHash == null) return 1;


var branchName = GetFromArgsOrEnv("Github Branch Name", 3, "BRANCH_NAME");

if (branchName == null) return 1;

if (branchName.StartsWith("ref/")) branchName = branchName.Replace("ref/", "");

var apiKey = GetFromArgsOrEnv("Upload API KEY", 4, "API_KEY");

if (apiKey == null) return 1;

var URL = GetFromArgsOrEnv("Upload Server URL", 5, "SERVER_URL");

if (URL == null) return 1;


client.DefaultRequestHeaders.Add("apiKey", apiKey);


var isPreviewDeploy = !(branchName.Equals("master", StringComparison.OrdinalIgnoreCase) ||
                        branchName.Equals("main", StringComparison.OrdinalIgnoreCase));


var uploadFileManifest =
    new UploadFileManifest(gitHash, hostName, new Dictionary<string, string>(), isPreviewDeploy);

long bytesUploaded = 0;


var files = Directory.GetFiles(filePath, "*", SearchOption.AllDirectories);

Console.WriteLine($"Uploading {files.Length} files....");


var options = new ProgressBarOptions
{
    ProgressCharacter = '─',
    ProgressBarOnBottom = true
};

var progressBar = new ProgressBar(files.Length, "Initial message", options);

for (var i = 0; i < files.Length; i++)
{
    var file = files[i];
#if DEBUG
    Console.WriteLine($"Reading {file}");
#endif
    progressBar.Tick($"Uploading {i}/{files.Length}");
    // https://stackoverflow.com/questions/38474362/get-a-file-sha256-hash-code-and-checksum
    using var sha256 = SHA256.Create();

    using var fileStream = File.OpenRead(file);

    var fileHash = BitConverter.ToString(await sha256.ComputeHashAsync(fileStream)).Replace("-", "")
        .ToLowerInvariant();

    // Try Get First

    var getFileAsync = await client.GetAsync($"{URL}{UploadFileEndpoint}/{fileHash}");

    if (getFileAsync.StatusCode != HttpStatusCode.NotFound && getFileAsync.IsSuccessStatusCode == false)
    {
        Console.WriteLine($"Error getting file metadata... aborting, got {getFileAsync.StatusCode} status Code");
        // Should be careful about this -- doing this will actually leak the headers of the request including the Api Key
        //Log.Logger.Fatal(await getFileAsync.Content.ReadAsStringAsync());
        return 1;
    }


    if (getFileAsync.StatusCode == HttpStatusCode.NotFound)
    {
        //https://stackoverflow.com/questions/19134062/encode-a-filestream-to-base64-with-c-sharp
        var bytes = new byte[(int)fileStream.Length];

        fileStream.Seek(0, SeekOrigin.Begin);
        await fileStream.ReadAsync(bytes, 0, (int)fileStream.Length);

        bytesUploaded += fileStream.Length;

        var Base64File = Convert.ToBase64String(bytes);

        var uploadFile = new UploadFileData(fileHash, Path.GetFileName(file), Base64File);

#if DEBUG
        Console.WriteLine($"Posting {JsonSerializer.Serialize(uploadFile)}");
#endif

        var result = await client.PostAsJsonAsync(URL + UploadFileEndpoint, uploadFile);

        if (result.StatusCode != HttpStatusCode.Accepted && result.StatusCode != HttpStatusCode.OK)
        {
            Console.WriteLine($"Error uploading file... aborting, got {result.StatusCode} status Code");
            // Should be careful about this -- doing this will actually leak the headers of the request including the Api Key
            //Log.Logger.Fatal(await result.Content.ReadAsStringAsync());
            return 1;
        }

        result.EnsureSuccessStatusCode();
#if DEBUG
        Console.WriteLine($"{file} - {fileHash} uploaded.");
#endif
    }
    else
    {
#if DEBUG

        Console.WriteLine($"{file} - {fileHash} is already uploaded.");
#endif
    }

    // We need to trim any leading slashes, get just the relative file path based off the uploading directory, and finally convert any slashes. Windows uses \ and Unix/Web/etc uses /, we will just switch to using /, just in case.
    var webFriendlyPath = file.Substring(filePath.Length).TrimStart('/', '\\').Replace("\\", "/");


    uploadFileManifest.URLHashDictionary.Add(webFriendlyPath, fileHash);
}

#if DEBUG
Console.WriteLine($"Posting {JsonSerializer.Serialize(uploadFileManifest)}");
#endif

progressBar.Tick("Uploading manifest");
var manifestResult = await client.PostAsJsonAsync(URL + UploadManifestEndpoint, uploadFileManifest);


if (manifestResult.StatusCode != HttpStatusCode.Accepted)
{
    Console.WriteLine($"Error uploading manifest... aborting, got {manifestResult.StatusCode} status Code");
    // Should be careful about this -- doing this will actually leak the headers of the request including the Api Key
    //Log.Logger.Fatal(await manifestResult.Content.ReadAsStringAsync());
    return 1;
}

progressBar.Dispose();
Console.WriteLine("Success!");
Console.WriteLine($"{uploadFileManifest.URLHashDictionary.Count} Files uploaded.");
Console.WriteLine($"{bytesUploaded} Bytes uploaded.");
if (uploadFileManifest.Preview)
    Console.WriteLine($"This should be viewable at https://{uploadFileManifest.GitHash}.{hostName}");
else
    Console.WriteLine(
        $"This should be viewable at https://{hostName} and https://{uploadFileManifest.GitHash}.{hostName}");

return 0;


string? GetFromArgsOrEnv(string name, int argNumber, string environmentVariableName = "")
{
    if (!string.IsNullOrWhiteSpace(environmentVariableName))
    {
        var getEnvironmentVariable = Environment.GetEnvironmentVariable(environmentVariableName);
        if (string.IsNullOrWhiteSpace(getEnvironmentVariable) == false) return getEnvironmentVariable;
    }

    if (args.Length - 1 < argNumber)
    {
        Console.WriteLine(
            $"Need {name} in {argNumber} parameter of command (or from environment variable in GA)");
        return null;
    }


    var getVariableFromArgs = args[argNumber];
    if (string.IsNullOrWhiteSpace(getVariableFromArgs))
    {
        Console.WriteLine(
            $"Need Valid {name} in {argNumber} parameter of command (or from environment variable in GA)");
        return null;
    }

    return getVariableFromArgs;
}