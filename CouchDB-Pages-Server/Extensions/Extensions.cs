using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text.Json;
using CouchDBPages.Server.Models.Data;
using Microsoft.AspNetCore.Connections.Features;
using Serilog;

namespace CouchDBPages.Server.Extensions;

public static class Extensions
{
    // We need case-insensitivity due to the differences between CouchDB and .NET's JSON Parsing
    public static readonly JsonSerializerOptions JsonSerializerOptions =
        new() { AllowTrailingCommas = true, PropertyNamingPolicy = null, PropertyNameCaseInsensitive = true };

    #region Header Helpers
    public static string TryResolveHeader(this HttpResponseHeaders headers, string header, string fallback = "")
    {
        var value = fallback;
        if (headers.TryGetValues(header, out var values)) value = values.First();

        return value;
    }

    public static string TryResolveHeader(this IHeaderDictionary headers, string header, string fallback = "")
    {
        var value = fallback;
        if (headers.TryGetValue(header, out var values)) value = values.First();

        return value;
    }

    #endregion


    #region PrettyPrint Used to Convert TLS info to NGINX-STYLE



    public static string PrettyPrintCiphersInfo(this ITlsHandshakeFeature? tlsHandshakeFeature)
    {
        List<string> prettyOutput = new List<string>();
        var keyExchangePrettyPrint = tlsHandshakeFeature?.KeyExchangeAlgorithm.PrettyPrint();
        if (string.IsNullOrWhiteSpace(keyExchangePrettyPrint) == false)
        {
            prettyOutput.Add(keyExchangePrettyPrint);
        }
        var cipherAlgoPrettyPrint = tlsHandshakeFeature?.CipherAlgorithm.PrettyPrint();
        if (string.IsNullOrWhiteSpace(cipherAlgoPrettyPrint) == false)
        {
            prettyOutput.Add(cipherAlgoPrettyPrint);
        }
        var hashAlgoPrettyPrint = tlsHandshakeFeature?.HashAlgorithm.PrettyPrint();
        if (string.IsNullOrWhiteSpace(hashAlgoPrettyPrint) == false)
        {
            prettyOutput.Add(hashAlgoPrettyPrint);
        }

        return String.Join("_", prettyOutput);
    }



    public static string PrettyPrint(this CipherAlgorithmType cipherAlgorithm)
    {
        switch (cipherAlgorithm)
        {
            case CipherAlgorithmType.Aes:
                return "AES_128";
            case CipherAlgorithmType.Aes128:
                return "AES_128";
            case CipherAlgorithmType.Aes192:
                return "AES_192";
            case CipherAlgorithmType.Aes256:
                return "AES_256";
            case CipherAlgorithmType.Des:
                return "DES";
            case CipherAlgorithmType.Rc2:
                return "REC2";
            case CipherAlgorithmType.Rc4:
                return "REC4";
            case CipherAlgorithmType.TripleDes:
                return "3DES";
            case CipherAlgorithmType.Null:
                return "";
            case CipherAlgorithmType.None:
                return "";
            default:
                return cipherAlgorithm.ToString();

        }
    }

    public static string PrettyPrint(this ExchangeAlgorithmType exchangeAlgorithm)
    {
        switch ((int)exchangeAlgorithm)
        {
            // 44550 == ECHDE
            //https://social.msdn.microsoft.com/Forums/sqlserver/en-US/d0298622-a7cc-40e8-a4bf-8b74696ff548/sslstreamkeyexchangealgorithm-44550?forum=netfxbcl
            //https://github.com/dotnet/runtime/issues/55570
            case 44550:
            case (int)ExchangeAlgorithmType.DiffieHellman:
                return "ECDHE";
            case (int)ExchangeAlgorithmType.RsaKeyX:
                return "RSA";
            case (int)ExchangeAlgorithmType.RsaSign:
                return "RSA";
            case (int)ExchangeAlgorithmType.None:
                return "TLS";
            default:
                return exchangeAlgorithm.ToString();


        }
    }
    public static string PrettyPrint(this HashAlgorithmType hashAlgorithmType)
    {
        switch (hashAlgorithmType)
        {
            case HashAlgorithmType.Md5:
                return "MD5";
            case HashAlgorithmType.Sha1:
                return "SHA1";
            case HashAlgorithmType.Sha256:
                return "SHA256";
            case HashAlgorithmType.Sha384:
                return "SHA384";
            case HashAlgorithmType.Sha512:
                return "SHA512";
            case HashAlgorithmType.None:
                return "";
            default:
                return hashAlgorithmType.ToString();
        }
    }


    #endregion



    #region HTTPClient Helpers

    public static async Task<T?> GetFromJsonAsyncSupportNull<T>(this HttpClient client, string? requestUri, CancellationToken token)
        where T : class
    {
        var response = await client.GetAsync(requestUri, token);
        response.ThrowForServerSideErrors();
        if (response.IsSuccessStatusCode)
        {
            var rawString = await response.Content.ReadAsStringAsync(token);


            if (string.IsNullOrWhiteSpace(rawString) == false)
            {
                var output = JsonSerializer.Deserialize<T>(rawString, JsonSerializerOptions);
                if (output is IValidator validation && validation.Validate() == false) return null;

                return output;
            }
        }

        return null;
    }

    public static void ThrowForServerSideErrors(this HttpResponseMessage msg)
    {
        if (msg.StatusCode != HttpStatusCode.NotFound && msg.StatusCode != HttpStatusCode.Conflict)
            msg.EnsureSuccessStatusCode();
    }

    #endregion

    #region Data Model Validation Helpers

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

    #endregion



}