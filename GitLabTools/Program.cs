using System.Diagnostics.CodeAnalysis;
using System.Net;
using CommandLine;
using Flurl.Http;
using Flurl.Http.Configuration;
using GitLabTools.Commandline;
using GitLabTools.GitLab;
using GitLabTools.Services;
using GitLabTools.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace GitLabTools;

[ExcludeFromCodeCoverage]
public static class Program
{
    private static ILogger? _logger;

    public static async Task<int> Main(string[] args)
    {
        return (int)await MainImplAsync(args);
    }

    private static async Task<ExitCodeTypes> MainImplAsync(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        using var scope = host.Services.CreateScope();
        _logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Program));
        try
        {
            var parsedResult =
                Parser.Default.ParseArguments<DeleteBuildPipelineArgument, ReadProjectInformationArgument, ReadGroupInformationArgument>(args);
            return await parsedResult.MapResult(
                (DeleteBuildPipelineArgument options) => DeleteBuildPipelineAsync(scope.ServiceProvider, options),
                (ReadProjectInformationArgument options) => ReadProjectInformationAsync(scope.ServiceProvider, options),
                (ReadGroupInformationArgument options) => ReadGroupInformationAsync(scope.ServiceProvider, options),
                _ => Task.FromResult(ExitCodeTypes.ErrorUnexpectedError));
        }
        catch (ArgumentValidationException ave)
        {
            _logger.LogError(ave, "Invalid arguments detected: {message}", ave.Message);
            return ExitCodeTypes.IllegalArguments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occured: {message}", ex.Message);
            return ExitCodeTypes.ErrorUnexpectedError;
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostingContext, logging) =>
            {
                // s. https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-6#4-microsoft-logging-filters
                logging.AddNLog(new NLogLoggingConfiguration(hostingContext.Configuration.GetSection("NLog")),
                    new NLogProviderOptions { RemoveLoggerFactoryFilter = false });
            })
            .ConfigureServices((_, services) =>
            {
                ConfigureServices(services);
            });

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<DeleteOldPipelinesService>();
        services.AddScoped<ReadProjectInformationService>();
        services.AddScoped<ReadGroupInformationService>();
        services.AddSingleton<IGitlabRestApiClient, GitLabRestApiClient>();
        services.AddSingleton(_ =>
        {
            var result = new FlurlClientCache()
                .WithDefaults(builder => builder.ConfigureInnerHandler(hch =>
                {
                    var proxyAddress = ProxyUtils.ReadProxyFromEnvironmentVariables();
                    if (string.IsNullOrWhiteSpace(proxyAddress))
                    {
                        return;
                    }
                    var bypassList = ProxyUtils.ReadNoProxyValueAsArrayFromEnvironmentVariables();
                    hch.Proxy = new WebProxy(proxyAddress, false, bypassList);
                    hch.UseProxy = true;
                }));

            result.Add(FlurClientNameConstants.GitLabClient, baseUrl: null, builder =>
            {
                builder.WithSettings(x =>
                {
                    x.Timeout = TimeSpan.FromSeconds(10);
                    x.HttpVersion = HttpVersion.Version20.ToString();
                });
            });
            return result;
        });
    }

    private static Task<ExitCodeTypes> DeleteBuildPipelineAsync(IServiceProvider serviceProvider, DeleteBuildPipelineArgument args)
    {
        DeleteBuildPipelineArgumentValidator.Validate(args);
        var deleteOldPipelinesService = serviceProvider.GetRequiredService<DeleteOldPipelinesService>();
        return deleteOldPipelinesService.DeleteBuildPipelineAsync(args);
    }

    private static Task<ExitCodeTypes> ReadProjectInformationAsync(IServiceProvider serviceProvider, ReadProjectInformationArgument args)
    {
        ReadProjectInformationArgumentValidator.Validate(args);
        var readProjectInformationService = serviceProvider.GetRequiredService<ReadProjectInformationService>();
        return readProjectInformationService.ReadProjectInformationAsync(args);
    }

    private static Task<ExitCodeTypes> ReadGroupInformationAsync(IServiceProvider serviceProvider, ReadGroupInformationArgument args)
    {
        ReadGroupInformationArgumentValidator.Validate(args);
        var readGroupInformationService = serviceProvider.GetRequiredService<ReadGroupInformationService>();
        return readGroupInformationService.ReadGroupInformationAsync(args);
    }
}