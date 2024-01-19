using System.Net;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using GitLabTools;
using GitLabTools.GitLab;
using GitLabTools.GitLab.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitlabTools.Tests.GitLab;

[TestClass]
public class GitLabRestApiClientTests
{
    private const string ExpectedGitLabUrl = "https://unittest.gitlab.blafasel.com";
    private const string ExpectedAccessToken = "AccessToken";
    private const int ExpectedGroupId = 4711;
    private const int ExpectedProjectId = 1147;
    private const int ExpectedPipelineId = 1417;

    private HttpTest _httpTest = null!;

    [TestInitialize]
    public void CreateHttpTest()
    {
        _httpTest = new HttpTest();
    }

    [TestCleanup]
    public void DisposeHttpTest()
    {
        _httpTest.Dispose();
    }

    #region ReadGroupAsync

    [TestMethod]
    public async Task ReadGroupAsync_GroupExists_ApiReturn200_CallExpectedMethods()
    {
        var expectedUri = $"{ExpectedGitLabUrl}/api/v4/groups/{ExpectedGroupId}";
        _httpTest.RespondWithJson(new Group
        {
            Id = ExpectedGroupId,
            Projects = [
                new Project
                {
                    Id = ExpectedProjectId
                }
            ]
        });

        var sut = CreateSut();
        var result = await sut.ReadGroupAsync(ExpectedGitLabUrl, ExpectedAccessToken, ExpectedGroupId);
        Assert.IsNotNull(result);
        Assert.AreEqual(ExpectedGroupId, result.Id);
        _httpTest.ShouldHaveCalled(expectedUri)
            .WithVerb(HttpMethod.Get)
            .WithHeader(GitLabRestApiClient.HeaderNamePrivateToken, ExpectedAccessToken);
    }

    [TestMethod]
    public async Task ReadGroupAsync_GroupDoesNotExists_ApiReturn404_Null()
    {
        _httpTest.RespondWith(status: (int)HttpStatusCode.NotFound);

        var sut = CreateSut();
        var result = await sut.ReadGroupAsync(ExpectedGitLabUrl, ExpectedAccessToken, ExpectedGroupId);
        Assert.IsNull(result);
    }

    [TestMethod]
    [ExpectedException(typeof(GitLabFailedException))]
    public async Task ReadGroupAsync_SimulateTimeout_ThrowException()
    {
        _httpTest.SimulateTimeout();

        var sut = CreateSut();
        await sut.ReadGroupAsync(ExpectedGitLabUrl, ExpectedAccessToken, ExpectedGroupId);
    }

    #endregion

    #region ReadProjectAsync

    [TestMethod]
    public async Task ReadProjectAsync_ProjectExists_ApiReturn200_CallExpectedMethods()
    {
        var expectedUri = $"{ExpectedGitLabUrl}/api/v4/projects/{ExpectedProjectId}";
        _httpTest.RespondWithJson(new Project
        {
            Id = ExpectedProjectId
        });

        var sut = CreateSut();
        var result = await sut.ReadProjectAsync(ExpectedGitLabUrl, ExpectedAccessToken, ExpectedProjectId);
        Assert.IsNotNull(result);
        Assert.AreEqual(ExpectedProjectId, result.Id);
        _httpTest.ShouldHaveCalled(expectedUri)
            .WithVerb(HttpMethod.Get)
            .WithHeader(GitLabRestApiClient.HeaderNamePrivateToken, ExpectedAccessToken);
    }

    [TestMethod]
    public async Task ReadProjectAsync_ProjectDoesNotExist_ApiReturn404_Null()
    {
        _httpTest.RespondWith(status: (int)HttpStatusCode.NotFound);

        var sut = CreateSut();
        var result = await sut.ReadProjectAsync(ExpectedGitLabUrl, ExpectedAccessToken, ExpectedProjectId);
        Assert.IsNull(result);
    }

    #endregion

    #region ReadAllPipelinesAsync

    [TestMethod]
    public async Task ReadAllPipelinesAsync_PipelinesExists_CallExpectedMethods()
    {
        var expectedUri = $"{ExpectedGitLabUrl}/api/v4/projects/{ExpectedProjectId}/pipelines";
        _httpTest
            .RespondWithJson(new Pipeline[]
            {
                new()
                {
                    Id = ExpectedPipelineId
                }
            })
            .RespondWithJson(Array.Empty<Pipeline>());

        var sut = CreateSut();
        var project = new Project
        {
            Id = ExpectedProjectId
        };
        var result = await sut.ReadAllPipelinesAsync(ExpectedGitLabUrl, ExpectedAccessToken, project);
        Assert.IsNotNull(result[0]);
        Assert.IsTrue(result[0].Id.HasValue);
        Assert.AreEqual(ExpectedPipelineId, result[0].Id);

        _httpTest.ShouldHaveCalled(expectedUri)
            .WithVerb(HttpMethod.Get)
            .WithHeader(GitLabRestApiClient.HeaderNamePrivateToken, ExpectedAccessToken)
            .WithQueryParam(GitLabRestApiClient.QueryParamNameSort, "desc")
            .WithQueryParam(GitLabRestApiClient.QueryParamNameOrderBy, "id")
            .WithQueryParam(GitLabRestApiClient.QueryParamNamePerPage, "100")
            .WithQueryParam(GitLabRestApiClient.QueryParamNamePage, 1);

        _httpTest.ShouldHaveCalled(expectedUri)
            .WithVerb(HttpMethod.Get)
            .WithHeader(GitLabRestApiClient.HeaderNamePrivateToken, ExpectedAccessToken)
            .WithQueryParam(GitLabRestApiClient.QueryParamNameSort, "desc")
            .WithQueryParam(GitLabRestApiClient.QueryParamNameOrderBy, "id")
            .WithQueryParam(GitLabRestApiClient.QueryParamNamePerPage, "100")
            .WithQueryParam(GitLabRestApiClient.QueryParamNamePage, 2);
    }

    #endregion

    #region ReadAllPipelinesAsync

    [TestMethod]
    public async Task DeletePipelinesAsync_DeletePipeline_CallExpectedMethods()
    {
        var expectedUri = $"{ExpectedGitLabUrl}/api/v4/projects/{ExpectedProjectId}/pipelines/{ExpectedPipelineId}";
        _httpTest
            .RespondWithJson(new Pipeline[]
            {
                new()
                {
                    Id = ExpectedPipelineId
                }
            })
            .RespondWithJson(Array.Empty<Pipeline>());

        var sut = CreateSut();
        var project = new Project
        {
            Id = ExpectedProjectId
        };
        var pipelines = new[]{
            new Pipeline
            {
                Id = ExpectedPipelineId
            }
        };
        await sut.DeletePipelinesAsync(ExpectedGitLabUrl, ExpectedAccessToken, project, pipelines);
        
        _httpTest.ShouldHaveCalled(expectedUri)
            .WithVerb(HttpMethod.Delete)
            .WithHeader(GitLabRestApiClient.HeaderNamePrivateToken, ExpectedAccessToken);
    }

    #endregion

    private static GitLabRestApiClient CreateSut()
    {
        var logger = new Mock<ILogger<GitLabRestApiClient>>();
        var flurlClientCache = new FlurlClientCache()
            .Add(FlurClientNameConstants.GitLabClient);
        return new GitLabRestApiClient(flurlClientCache, logger.Object);
    }
}
