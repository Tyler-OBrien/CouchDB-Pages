using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace CouchDBPages.GenericDataSync.Helpers;

public class LogHandler
{
    private const string outputFormat =
        "[{Timestamp:h:mm:ss ff tt}] [{Level:u3}] {Message:lj} {Exception:j}{NewLine}";

    public static Logger Init()
    {
        var Logger = new LoggerConfiguration().WriteTo.Async(config =>
        {
            config.File("Logs/Logger.log",
                LogEventLevel.Information, outputFormat, rollingInterval: RollingInterval.Day);
            config.Console(outputTemplate: outputFormat);
        }).Enrich.FromLogContext().CreateLogger();
        Log.Logger = Logger;
        Log.Logger.Information("Loaded SeriLog Logger");
        return Logger;
    }
}