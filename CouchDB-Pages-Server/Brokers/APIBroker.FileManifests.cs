﻿using CouchDBPages.Server.Extensions;
using CouchDBPages.Server.Models.Data;

namespace CouchDBPages.Server.Brokers;

public partial class APIBroker
{
    public readonly string filesManifestDB;


    public async Task<PagesFileManifest?> FindManifestAsync(string id)
    {
        return await _httpClient.GetFromJsonAsyncSupportNull<PagesFileManifest>(
            $"/{filesManifestDB}/{Uri.EscapeDataString(id)}");
    }

    public async Task<HttpResponseMessage> PutManifestAsync(PagesFileManifest manifest)
    {
        return await _httpClient.PutAsJsonAsync($"/{filesManifestDB}/{Uri.EscapeDataString(manifest.ID)}", manifest);
    }
}