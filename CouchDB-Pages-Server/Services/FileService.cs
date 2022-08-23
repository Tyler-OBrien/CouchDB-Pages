using CouchDBPages.Server.Models.Responses;
using CouchDBPages.Server.Models.Services;

namespace CouchDBPages.Server.Services;

public class FileService : IFileService
{
    private readonly IFileDataManifestService _fileDataManifestService;
    private readonly IFileDataService _fileDataService;
    private readonly ILogger _logger;


    public FileService(
        IFileDataManifestService fileDataManifestService, IFileDataService fileDataService, ILogger<FileService> logger)
    {
        _fileDataManifestService = fileDataManifestService;
        _fileDataService = fileDataService;
        _logger = logger;
    }

    public async Task<FileDataResponse?> GetFile(string hostName, string path, CancellationToken token)
    {
        // Even if the request is cancelled, we should still cache the metadata
        var findManifest = await _fileDataManifestService.GetMetadata(hostName, CancellationToken.None);
        if (findManifest == null)
        {
#if DEBUG
            _logger.LogInformation($"Could not find Manifest for {hostName}");
#endif
            return null;
        }
        

        // Remove the first slash

        // Handle rewriting for empty paths
        if (path.EndsWith("/", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(path))
            path += "index.html";


        if (findManifest.URLHashDictionary.TryGetValue(path.Trim(), out var fileHashValue) == false)
        {
#if DEBUG
            _logger.LogInformation($"Could not find hash value in manifest for {path} and hostname {hostName}");
            _logger.LogInformation($"Full Manifest Options: {string.Join(",", findManifest.URLHashDictionary)}");
#endif
            return null;
        }


        return await _fileDataService.GetFile(fileHashValue, token);
    }
}