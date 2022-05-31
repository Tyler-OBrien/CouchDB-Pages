using System.Text.Json;

namespace CouchDBPages.GenericDataSync.Extensions;

public static class Extensions
{
    // We need case-insensitivity due to the differences between CouchDB and .NET's JSON Parsing
    public static readonly JsonSerializerOptions JsonSerializerOptions =
        new() { AllowTrailingCommas = true, PropertyNamingPolicy = null, PropertyNameCaseInsensitive = true };

    public static async Task<T?> GetFromJsonAsyncSupportNull<T>(this HttpClient client, string? requestUri)
        where T : class
    {
        var response = await client.GetAsync(requestUri);
        if (response.IsSuccessStatusCode)
        {
            var rawString = await response.Content.ReadAsStringAsync();


            if (string.IsNullOrWhiteSpace(rawString) == false)
                return JsonSerializer.Deserialize<T>(rawString, JsonSerializerOptions);
        }

        return null;
    }
}