using System.Text.Json;
using CouchDBPages.GenericDataSync.Models;
using Serilog;

namespace CouchDBPages.GenericDataSync.Helpers;

public class SimpleConfigurationHandler
{
    public const string ConfigName = "SyncConfig.json";

    public static JsonSerializerOptions JsonSerializerOptions = new()
        { AllowTrailingCommas = true, PropertyNamingPolicy = null, WriteIndented = true };

    public static Configuration SetupOrReadConfig()
    {
        var Config = new Configuration();
        var ResetConfig = false;
        // read file into a string and deserialize JSON to a type
        if (File.Exists(ConfigName) == false)
            ResetConfig = true;
        else
            try
            {
                Config = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(ConfigName),
                    JsonSerializerOptions);
                if (Config == null) throw new Exception("Config shouldn't be null, error reading..");
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Error Reading config, resetting.");
                ResetConfig = true;
            }

        if (ResetConfig)
        {
            Log.Logger.Information("Generating default configuration");
            var Defaultconfig = ReturnDefaultConfiguration();
            File.WriteAllText(ConfigName,
                JsonSerializer.Serialize(Defaultconfig, JsonSerializerOptions));
            Config = Defaultconfig;
        }

        Log.Logger.Information($"Running with config, CouchDB Address: {Config.CouchDB_URL}");
        return Config;
    }

    public static Configuration ReturnDefaultConfiguration()
    {
        return new Configuration
        {
            CouchDB_Password = "Password",
            CouchDB_URL = "http://localhost:5984",
            CouchDB_Username = "UserName",
            RoleTypeInfo = "Reading",
            Database = "secrets",
            FilesToHandle = new List<FilesToHandle>
            {
                new()
                {
                    FileName = "example.html",
                    FilePath = "Inner-folder/example.html",
                    UnixProtected = false
                },
                new()
                {
                    FileName = "test.gihgifodhdfgoih.txt",
                    FilePath = "t54ihoighgfdoihfdg.txt",
                    UnixProtected = true
                }
            },
            Sentry_DSN = ""
        };
    }
}