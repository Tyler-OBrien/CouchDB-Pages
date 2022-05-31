using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using CouchDBPages.Server.Models.Data;
using Serilog;

namespace CouchDBPages.Server.Extensions;

public static class Extensions
{
    // We need case-insensitivity due to the differences between CouchDB and .NET's JSON Parsing
    public static readonly JsonSerializerOptions JsonSerializerOptions =
        new() { AllowTrailingCommas = true, PropertyNamingPolicy = null, PropertyNameCaseInsensitive = true };

    public static string TryResolveHeader(this HttpResponseHeaders headers, string header, string fallback = "")
    {
        var value = fallback;
        if (headers.TryGetValues(header, out var values)) value = values.First();

        return value;
    }

    public static async Task<T?> GetFromJsonAsyncSupportNull<T>(this HttpClient client, string? requestUri)
        where T : class
    {
        var response = await client.GetAsync(requestUri);
        response.ThrowForServerSideErrors();
        if (response.IsSuccessStatusCode)
        {
            var rawString = await response.Content.ReadAsStringAsync();


            if (string.IsNullOrWhiteSpace(rawString) == false)
            {
                var output = JsonSerializer.Deserialize<T>(rawString, JsonSerializerOptions);
                if (output is IValidator validation && validation.Validate() == false) return null;

                return output;
            }
        }

        return null;
    }


    public static bool Validate(this string? data)
    {
        return data != null && string.IsNullOrWhiteSpace(data) == false;
    }

    public static bool Validate(this object? data)
    {
        return data != null;
    }

    public static bool Validate(params (bool condition, string name)[] validations)
    {
        foreach (var validation in validations)
            if (validation.condition == false)
            {
#if DEBUG
                Log.Debug($"Validation failed {validation.name}");
#endif
                return false;
            }

        return true;
    }

    public static async Task VerifyCouchOk(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode == false)
        {
            // Strange.......
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            response.EnsureSuccessStatusCode();
        }
    }

    public static void ThrowForServerSideErrors(this HttpResponseMessage msg)
    {
        if (msg.StatusCode != HttpStatusCode.NotFound && msg.StatusCode != HttpStatusCode.Conflict)
            msg.EnsureSuccessStatusCode();
    }
}