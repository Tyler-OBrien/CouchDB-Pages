using CouchDBPages.Server.Brokers;
using CouchDBPages.Server.Middleware;
using CouchDBPages.Server.Models.Config;
using CouchDBPages.Server.Models.Services;
using CouchDBPages.Server.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Polly;
using Polly.Extensions.Http;
using Prometheus;
using Sentry.Extensibility;
using Serilog;
using Serilog.Events;

namespace CouchDBPages.Server;

public class Program
{
    private const string outputFormat =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj} {Exception}{NewLine}";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var applicationConfig = builder.Configuration.GetSection(ApplicationConfig.Section).Get<ApplicationConfig>();

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
        Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information).WriteTo.Async(config =>
            {
                config.File($"Logs/Log{timestamp}.log", outputTemplate: outputFormat,
                    restrictedToMinimumLevel: LogEventLevel.Information);
                config.Console(outputTemplate: outputFormat, restrictedToMinimumLevel: LogEventLevel.Information);
            }).Enrich.FromLogContext().CreateLogger();
        Log.Logger.Information("Loaded SeriLog Logger");


        builder.Host.UseSerilog();
        if (string.IsNullOrWhiteSpace(applicationConfig.SENTRY_DSN) == false)
            builder.WebHost.UseSentry(options =>
            {
                options.Dsn = applicationConfig.SENTRY_DSN;
                options.SendDefaultPii = true;
                options.AttachStacktrace = true;
                options.MaxRequestBodySize = RequestSize.Always;
                options.MinimumBreadcrumbLevel = LogLevel.Debug;
                options.MinimumEventLevel = LogLevel.Warning;
            });

        // Add services to the container.
        // Add functionality to inject IOptions<T>
        builder.Services.AddOptions();
        builder.Services.AddLogging();
        builder.Services.AddResponseCaching(options =>
        {
            // 8MB max
            options.MaximumBodySize = 8388608;
            options.UseCaseSensitivePaths = true;
        });



        builder.Services.Configure<ApplicationConfig>(
            builder.Configuration.GetSection(ApplicationConfig.Section));


        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        builder.Services.AddScoped<IFileDataManifestService, FileDataManifestService>();
        builder.Services.AddScoped<IFileDataService, FileDataService>();
        builder.Services.AddScoped<IFileService, FileService>();
        builder.Services.AddScoped<ISecretsService, SecretsService>();

        builder.Services.AddScoped<IAPIBroker, APIBroker>();
        builder.Services.AddHttpClient<IAPIBroker, APIBroker>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());
        builder.Services.AddScoped<HeaderMiddleware>();

        builder.WebHost.UseKestrel(options => { options.AddServerHeader = false; });


        builder.Services.AddMemoryCache(options =>
        {
            // ~341 MiB of Ram
            options.SizeLimit = 357913941;
        });

        var app = builder.Build();

        if (applicationConfig.Prometheus_Metrics_Port != default )
        {
            Log.Logger.Information($"Enabling Prometheus Metrics at port {applicationConfig.Prometheus_Metrics_Port}.");
            app.UseMetricServer(applicationConfig.Prometheus_Metrics_Port);
        }

        if (applicationConfig.Behind_Reverse_Proxy)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
        }

        //app.UseSerilogRequestLogging();

        app.UseMiddleware<HeaderMiddleware>();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }



        app.UseHttpsRedirection();
        app.UseResponseCaching();
        app.UseAuthorization();

        app.MapControllers();

        app.UseRouting();

        app.MapControllerRoute(
            "Default",
            "{*url}",
            new { controller = "CouchDB", action = "GetFromDatabase" }
        );

        if (applicationConfig.Prometheus_Metrics_Port != default)
        {
            app.UseHttpMetrics();
        }
       

        // We wouldn't flush any errors above to Log, but Sentry should be fine for that.
        try
        {
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Web Server died");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromMilliseconds(Math.Max(50, retryAttempt * 50)));
    }
}