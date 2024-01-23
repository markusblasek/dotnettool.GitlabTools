using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using GitLabTools.GitLabRestClient.Models.V4;
using Microsoft.Extensions.Logging;
using Polly;

namespace GitLabTools.GitLabRestClient;

public class GitLabRestApiClient(IFlurlClientCache flurlClientCache, ILogger<GitLabRestApiClient> logger)
    : IGitlabRestApiClient
{
    public const string HeaderNamePrivateToken = "PRIVATE-TOKEN";
    public const string QueryParamNamePage = "page";
    public const string QueryParamNamePerPage = "per_page";
    public const string QueryParamNameOrderBy = "order_by";
    public const string QueryParamNameSort = "sort";
    private const int MaxResultsPerPage = 100;

    private readonly IFlurlClient _client = flurlClientCache.Get(FlurClientNameConstants.GitLabClient);

    private readonly IAsyncPolicy _asyncPolicyForExecuteHttpRequestsToGitlabCi =
        PollyUtils.GetAsyncRetryPolicyForExecuteHttpRequests();

    /// <inheritdoc />
    public async Task<Project?> ReadProjectAsync(
        string gitlabUrl, string accessToken, int projectId, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var client = ConfigureClient(gitlabUrl, accessToken);
            return await ReadProjectFromGitlabAsync(client, projectId, cancellationToken);
        }
        finally
        {
            sw.Stop();
            logger.LogDebug("Duration {methodName}({arg1}, {arg2}, {arg3}, {arg4}): {elapsedMilliseconds} ms",
                nameof(ReadProjectAsync), typeof(string), typeof(string), typeof(int), typeof(CancellationToken), sw.ElapsedMilliseconds);
        }
    }

    private async Task<Project?> ReadProjectFromGitlabAsync(
        IFlurlClient client, int projectId, CancellationToken cancellationToken)
    {
        try
        {
            var url = client.BaseUrl.AppendPathSegments("api", "v4", "projects", projectId);
            return await ExecuteRequestWithJsonResponseBodyAsync<Project, GitLabFailedException>(
                _asyncPolicyForExecuteHttpRequestsToGitlabCi, client, HttpMethod.Get, url, null, cancellationToken);
        }
        catch (GitLabFailedException gfe)
        {
            if (gfe.InnerException is FlurlHttpException { StatusCode: (int)HttpStatusCode.NotFound })
            {
                return null;
            }
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Group?> ReadGroupAsync(string gitlabUrl, string accessToken, int groupId,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var client = ConfigureClient(gitlabUrl, accessToken);
            return await ReadGroupFromGitlabAsync(client, groupId, cancellationToken);
        }
        finally
        {
            sw.Stop();
            logger.LogDebug("Duration {methodName}({arg1}, {arg2}, {arg3}, {arg4}): {elapsedMilliseconds} ms",
                nameof(ReadGroupAsync), typeof(string), typeof(string), typeof(int), typeof(CancellationToken), sw.ElapsedMilliseconds);
        }
    }

    private async Task<Group?> ReadGroupFromGitlabAsync(
        IFlurlClient client, int groupId, CancellationToken cancellationToken)
    {
        try
        {
            var url = client.BaseUrl.AppendPathSegments("api", "v4", "groups", groupId);
            return await ExecuteRequestWithJsonResponseBodyAsync<Group, GitLabFailedException>(
                _asyncPolicyForExecuteHttpRequestsToGitlabCi, client, HttpMethod.Get, url, null, cancellationToken);
        }
        catch (GitLabFailedException gfe)
        {
            if (gfe.InnerException is FlurlHttpException { StatusCode: (int)HttpStatusCode.NotFound })
            {
                return null;
            }
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Pipeline[]> ReadAllPipelinesAsync(
        string gitlabUrl, string accessToken, Project project, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var client = ConfigureClient(gitlabUrl, accessToken);
            return await ReadAllPipelinesFromGitlabAsync(client, project, cancellationToken);
        }
        finally
        {
            sw.Stop();
            logger.LogDebug("Duration {methodName}({arg1}, {arg2}, {arg3}, {arg4}): {elapsedMilliseconds} ms",
                nameof(ReadAllPipelinesAsync), typeof(string), typeof(string), typeof(int), typeof(CancellationToken), sw.ElapsedMilliseconds);
        }
    }

    private async Task<Pipeline[]> ReadAllPipelinesFromGitlabAsync(
        IFlurlClient client, Project project, CancellationToken cancellationToken)
    {
        var result = new List<Pipeline>();
        var pageNum = 1;
        var paginatedResult = await ReadPipelinesFromGitlabAsync(client, project, pageNum, cancellationToken);
        var pagesTotal = paginatedResult.PagesTotal;
        while (paginatedResult.Data?.Length > 0)
        {
            result.AddRange(paginatedResult.Data);
            if (pageNum >= pagesTotal)
            {
                break;
            }
            paginatedResult = await ReadPipelinesFromGitlabAsync(client, project, ++pageNum, cancellationToken);
        }
        return result.Where(x => x.Id.HasValue && !string.IsNullOrWhiteSpace(x.Status)).ToArray();
    }

    private async Task<PaginatedResult<Pipeline>> ReadPipelinesFromGitlabAsync(
        IFlurlClient client, Project project, int pageNum, CancellationToken cancellationToken)
    {
        var jsonPropertyNameId = ExpressionUtils.GetJsonPropertyName(() => new Pipeline().Id);
        var url = client.BaseUrl
            .AppendPathSegments("api", "v4", "projects", project.Id, "pipelines")
            .SetQueryParam(QueryParamNamePage, pageNum)
            .SetQueryParam(QueryParamNamePerPage, MaxResultsPerPage)
            .SetQueryParam(QueryParamNameOrderBy, jsonPropertyNameId)
            .SetQueryParam(QueryParamNameSort, "desc");

        try
        {
            var response = await ExecuteRequestAsync<GitLabFailedException>(
                _asyncPolicyForExecuteHttpRequestsToGitlabCi, client,
                HttpMethod.Get, url, null, cancellationToken);
            var result = new PaginatedResult<Pipeline>();
            if (response.Headers.TryGetFirst("x-page", out var pageNumAsString) && int.TryParse(pageNumAsString, out var parsedPageNum))
            {
                result.PageNum = parsedPageNum;
            }

            if (response.Headers.TryGetFirst("x-total-pages", out var totalPagesAsString) && int.TryParse(totalPagesAsString, out var parsedTotalPage))
            {
                result.PagesTotal = parsedTotalPage;
            }

            if (response.Headers.TryGetFirst("x-per-page", out var perPageAsString) && int.TryParse(perPageAsString, out var parsedPerPage))
            {
                result.PerPage = parsedPerPage;
            }

            if (response.Headers.TryGetFirst("x-total", out var totalAsString) && int.TryParse(totalAsString, out var parsedTotal))
            {
                result.Total = parsedTotal;
            }

            result.Data = await response.GetJsonAsync<Pipeline[]>().ConfigureAwait(false);
            return result;
        }
        catch (FlurlHttpException ex)
        {
            throw await HandleFlurlExceptionAsync<GitLabFailedException>(ex);
        }
    }

    /// <inheritdoc />
    public async Task DeletePipelinesAsync(string gitlabUrl, string accessToken, Project project, Pipeline[] pipelinesToDelete, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var client = ConfigureClient(gitlabUrl, accessToken);
            await DeletePipelinesFromGitlabAsync(client, project, pipelinesToDelete, cancellationToken);
        }
        finally
        {
            sw.Stop();
            logger.LogDebug("Duration {methodName}({arg1}, {arg2}, {arg3}, {arg4}, {arg5}): {elapsedMilliseconds} ms",
                nameof(DeletePipelinesAsync), typeof(string), typeof(string), typeof(Project), typeof(Pipeline[]), typeof(CancellationToken), sw.ElapsedMilliseconds);
        }
    }

    private async Task DeletePipelinesFromGitlabAsync(IFlurlClient client, Project project, IEnumerable<Pipeline> pipelinesToDelete, CancellationToken cancellationToken)
    {
        foreach (var pipelineId in pipelinesToDelete.Select(x => x.Id))
        {
            logger.LogTrace("Deleting old pipeline {pipelineId} from project id '{projectId}' ('{projectName}')",
                pipelineId, project.Id, project.Name);
            var url = client.BaseUrl.AppendPathSegments("api", "v4", "projects", project.Id, "pipelines", pipelineId);
            await ExecuteRequestWithNoResponseBodyAsync<GitLabFailedException>(
                _asyncPolicyForExecuteHttpRequestsToGitlabCi, client, HttpMethod.Delete, url, null, cancellationToken);
            logger.LogTrace("Old pipeline with id '{pipelineId}' from project id '{projectId}' ('{projectName}') deleted successfully",
                pipelineId, project.Id, project.Name);
        }
    }

    private IFlurlClient ConfigureClient(string gitlabUrl, string accessToken)
    {
        return ConfigureClient(_client, gitlabUrl, accessToken);
    }

    private static IFlurlClient ConfigureClient(IFlurlClient client, string gitlabUrl, string accessToken)
    {
        client.BaseUrl = gitlabUrl;
        client.Headers.AddOrReplace(HeaderNamePrivateToken, accessToken);
        return client;
    }

    /// <summary>
    /// Executes the request and returns the parsed json http body
    /// </summary>
    /// <typeparam name="T">Exception to throw if an error occured</typeparam>
    /// <param name="policy"></param>
    /// <param name="flurlClient"></param>
    /// <param name="method"></param>
    /// <param name="url"></param>
    /// <param name="content"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<IFlurlResponse> ExecuteRequestAsync<T>(
        IAsyncPolicy policy, IFlurlClient flurlClient, HttpMethod method, Url url, object? content, CancellationToken cancellationToken)
        where T : Exception
    {
        try
        {
            return await policy
                .ExecuteAsync(async token => await flurlClient
                    .Request(url)
                    .SendJsonAsync(method, content, HttpCompletionOption.ResponseContentRead, token)
                    .ConfigureAwait(false), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (FlurlHttpException ex)
        {
            throw await HandleFlurlExceptionAsync<T>(ex);
        }
    }

    /// <summary>
    /// Executes the request and returns the parsed json http body
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TU">Exception to throw if an error occured</typeparam>
    /// <param name="policy"></param>
    /// <param name="flurlClient"></param>
    /// <param name="method"></param>
    /// <param name="url"></param>
    /// <param name="content"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<T> ExecuteRequestWithJsonResponseBodyAsync<T, TU>(
        IAsyncPolicy policy, IFlurlClient flurlClient, HttpMethod method, Url url, object? content, CancellationToken cancellationToken)
        where T : class where TU : Exception
    {
        try
        {
            return await policy
                .ExecuteAsync(async token => await flurlClient
                    .Request(url)
                    .SendJsonAsync(method, content, HttpCompletionOption.ResponseContentRead, token)
                    .ReceiveJson<T>()
                    .ConfigureAwait(false), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (FlurlHttpException ex)
        {
            throw await HandleFlurlExceptionAsync<TU>(ex);
        }
    }

    /// <summary>
    /// Executes the request (for methods with no http body like delete methods etc.)
    /// </summary>
    /// <typeparam name="TU">Exception to throw if an error occured</typeparam>
    private static async Task ExecuteRequestWithNoResponseBodyAsync<TU>(
        IAsyncPolicy policy, IFlurlClient flurlClient, HttpMethod method, Url url, object? content, CancellationToken cancellationToken)
        where TU : Exception
    {
        try
        {
            await policy
                .ExecuteAsync(async token => await flurlClient
                    .Request(url)
                    .SendJsonAsync(method, content, HttpCompletionOption.ResponseContentRead, token)
                    .ConfigureAwait(false), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (FlurlHttpException ex)
        {
            throw await HandleFlurlExceptionAsync<TU>(ex);
        }
    }

    private static async Task<T> HandleFlurlExceptionAsync<T>(FlurlHttpException ex) where T : Exception
    {
        var message = ex is FlurlHttpTimeoutException
            ? $"Timeout reached when call {ex.Call.Request.Url}: {ex.Message}"
            : $"Unexpected error (HttpStatusCode: {ex.StatusCode}) returned from {ex.Call.Request.Url} (HttpMethod: {ex.Call.Request.Verb}): "
              + $"{ex.Message}{Environment.NewLine}HttpResponse: {await ex.GetResponseStringAsync()}";
        return CreateException<T>(message, ex);
    }

    [ExcludeFromCodeCoverage]
    private static T CreateException<T>(string message, Exception ex) where T : Exception
    {
        var type = typeof(T);
        var ctor = type.GetConstructor([typeof(string), typeof(Exception)])
                   ?? throw new InvalidOperationException($"Constructor with {typeof(string)} and {typeof(Exception)} does not exist");
        return (T)ctor.Invoke([message, ex]);
    }
}
