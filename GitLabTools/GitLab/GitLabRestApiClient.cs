using System.Net;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using GitLabTools.GitLab.Models;
using Microsoft.Extensions.Logging;
using Polly;

namespace GitLabTools.GitLab;

public class GitLabRestApiClient(IFlurlClientCache flurlClientCache, ILogger<GitLabRestApiClient> logger)
    : IGitlabRestApiClient
{
    private readonly IFlurlClient _client = flurlClientCache.Get(FlurClientNameConstants.GitLabClient);

    private readonly IAsyncPolicy _asyncPolicyForExecuteHttpRequestsToGitlabCi =
        PollyUtils.GetAsyncRetryPolicyForExecuteHttpRequests();

    /// <inheritdoc />
    public Task<Project?> ReadProjectAsync(
        string gitlabUrl, string accessToken, int projectId, CancellationToken cancellationToken = default)
    {
        var client = AddCredentialsToClient(gitlabUrl, accessToken);
        return ReadProjectFromGitlabAsync(client, projectId, cancellationToken);
    }

    private async Task<Project?> ReadProjectFromGitlabAsync(
        IFlurlClient client, int projectId, CancellationToken cancellationToken)
    {
        try
        {
            var url = client.BaseUrl.AppendPathSegments("api", "v4", "projects", projectId);
            return await ExecuteRequestWithJsonResponseBodyAsync<Project, GitlabCiFailedException>(
                _asyncPolicyForExecuteHttpRequestsToGitlabCi, client, HttpMethod.Get, url, null, cancellationToken);
        }
        catch (GitlabCiFailedException gfe)
        {
            if (gfe.InnerException is FlurlHttpException { StatusCode: (int)HttpStatusCode.NotFound })
            {
                return null;
            }
            throw;
        }
    }

    /// <inheritdoc />
    public Task<Pipeline[]> ReadAllPipelinesAsync(
        string gitlabUrl, string accessToken, Project project, CancellationToken cancellationToken)
    {
        var client = AddCredentialsToClient(gitlabUrl, accessToken);
        return ReadAllPipelinesFromGitlabAsync(client, project, cancellationToken);
    }

    private async Task<Pipeline[]> ReadAllPipelinesFromGitlabAsync(
        IFlurlClient client, Project project, CancellationToken cancellationToken)
    {
        var result = new List<Pipeline>();
        var pageNum = 1;
        var pipelines = await ReadPipelinesFromGitlabAsync(client, project, pageNum, cancellationToken);
        while (pipelines is { Length: > 0 })
        {
            result.AddRange(pipelines);
            pipelines = await ReadPipelinesFromGitlabAsync(client, project, ++pageNum, cancellationToken);
        }
        return result.Where(x => x.Id.HasValue).ToArray();
    }

    private async Task<Pipeline[]> ReadPipelinesFromGitlabAsync(
        IFlurlClient client, Project project, int pageNum, CancellationToken cancellationToken)
    {
        var url = client.BaseUrl
            .AppendPathSegments("api", "v4", "projects", project.Id, "pipelines")
            .SetQueryParam("page", pageNum)
            .SetQueryParam("per_page", "100")
            .SetQueryParam("order_by", "id")
            .SetQueryParam("sort", "desc");
        return await ExecuteRequestWithJsonResponseBodyAsync<Pipeline[], GitlabCiFailedException>(
            _asyncPolicyForExecuteHttpRequestsToGitlabCi, client, HttpMethod.Get, url, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task DeletePipelinesAsync(string gitlabUrl, string accessToken, Project project, Pipeline[] pipelinesToDelete, CancellationToken cancellationToken)
    {
        var client = AddCredentialsToClient(gitlabUrl, accessToken);
        return DeletePipelinesFromGitlabAsync(client, project, pipelinesToDelete, cancellationToken);
    }

    private async Task DeletePipelinesFromGitlabAsync(IFlurlClient client, Project project, IEnumerable<Pipeline> pipelinesToDelete, CancellationToken cancellationToken)
    {
        foreach (var pipelineId in pipelinesToDelete.Select(x => x.Id))
        {
            logger.LogTrace("Deleting old pipeline {pipelineId} from project id '{projectId}' ('{projectName}')",
                pipelineId, project.Id, project.Name);
            var url = client.BaseUrl.AppendPathSegments("api", "v4", "projects", project.Id, "pipelines", pipelineId);
            await ExecuteRequestWithNoResponseBodyAsync<GitlabCiFailedException>(
                _asyncPolicyForExecuteHttpRequestsToGitlabCi, client, HttpMethod.Delete, url, null, cancellationToken);
            logger.LogTrace("Old pipeline with id '{pipelineId}' from project id '{projectId}' ('{projectName}') deleted successfully",
                pipelineId, project.Id, project.Name);
        }
    }

    private IFlurlClient AddCredentialsToClient(string gitlabUrl, string accessToken)
    {
        _client.BaseUrl = gitlabUrl;
        _client.Headers.AddOrReplace("PRIVATE-TOKEN", accessToken);
        return _client;
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
                    .ReceiveJson<T>(), cancellationToken).ConfigureAwait(false);
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
                    .SendJsonAsync(method, content, HttpCompletionOption.ResponseContentRead, token), cancellationToken).ConfigureAwait(false);
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

    private static T CreateException<T>(string message, Exception ex) where T : Exception
    {
        var type = typeof(T);
        var ctor = type.GetConstructor([typeof(string), typeof(Exception)])
                   ?? throw new InvalidOperationException($"Constructor with {typeof(string)} and {typeof(Exception)} does not exist");
        return (T)ctor.Invoke([message, ex]);
    }
}
