using GitLabTools.Commandline;
using GitLabTools.GitLab;
using GitLabTools.GitLab.Models;
using GitLabTools.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace GitlabTools.Tests.Services;

[TestClass]
public class DeleteOldPipelinesServiceTests
{
    private const string ExpectedAccessToken = "accessToken";
    private const string ExpectedGitLabUrl = "https://blafasel.unittest.com";
    private const int ExpectedGroupId = 4711;
    private const int ExpectedProjectId = 1147;
    private const int ExpectedPipelineId1 = 1417;
    private const int ExpectedPipelineId2 = 1416;
    private static readonly DateTime ExpectedUtcNow = new(2020, 12, 24, 0, 0, 1, DateTimeKind.Utc);
    private static readonly DateTime ExpectedPipeline1CreatedAtUtc = new(2020, 12, 22, 0, 0, 1, DateTimeKind.Utc);
    private static readonly DateTime ExpectedPipeline2CreatedAtUtc = new(2020, 12, 23, 0, 0, 1, DateTimeKind.Utc);

    [TestMethod]
    public async Task ReadGroupInformationAsync_GroupExists_CallExpectedMethods()
    {
        var mockGitLabRestApiClient = new Mock<IGitlabRestApiClient>();
        mockGitLabRestApiClient
            .Setup(x =>
                x.ReadGroupAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Group
            {
                Id = ExpectedGroupId,
                Projects = [
                    new Project
                    {
                        Id = ExpectedProjectId
                    }
                ]
            });
        mockGitLabRestApiClient.Setup(x =>
                x.ReadAllPipelinesAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Pipeline
                {
                    Id = ExpectedPipelineId1,
                    CreatedAt = ExpectedPipeline1CreatedAtUtc
                }
            ]);
        var sut = CreateSut(mockGitLabRestApiClient.Object);
        var args = CreateArgumentsWithGroupId();
        var result = await sut.DeleteBuildPipelineAsync(args);
        Assert.AreEqual(result, ExitCodeTypes.Ok);
        mockGitLabRestApiClient
            .Verify(x => x.ReadGroupAsync(ExpectedGitLabUrl, ExpectedAccessToken, ExpectedGroupId, It.IsAny<CancellationToken>())
                , Times.Once);
        mockGitLabRestApiClient
            .Verify(x => x.ReadAllPipelinesAsync(ExpectedGitLabUrl, ExpectedAccessToken, 
                    It.Is<Project>(y => y.Id == ExpectedProjectId), It.IsAny<CancellationToken>())
                , Times.Once);
        mockGitLabRestApiClient
            .Verify(x => x.DeletePipelinesAsync(ExpectedGitLabUrl, ExpectedAccessToken,
                    It.Is<Project>(y => y.Id == ExpectedProjectId), It.Is<Pipeline[]>(y => VerifyPipelines(y)), It.IsAny<CancellationToken>())
                , Times.Once);
    }

    [TestMethod]
    public async Task ReadGroupInformationAsync_GroupExists_DryRunIsEnabled_CallExpectedMethods()
    {
        var mockGitLabRestApiClient = new Mock<IGitlabRestApiClient>();
        mockGitLabRestApiClient
            .Setup(x =>
                x.ReadGroupAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Group
            {
                Id = ExpectedGroupId,
                Projects = [
                    new Project
                    {
                        Id = ExpectedProjectId
                    }
                ]
            });
        mockGitLabRestApiClient.Setup(x =>
                x.ReadAllPipelinesAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Pipeline
                {
                    Id = ExpectedPipelineId1,
                    CreatedAt = ExpectedPipeline1CreatedAtUtc
                }
            ]);
        var sut = CreateSut(mockGitLabRestApiClient.Object);
        var args = CreateArgumentsWithGroupId();
        args.DryRun = true;
        var result = await sut.DeleteBuildPipelineAsync(args);
        Assert.AreEqual(result, ExitCodeTypes.Ok);
        mockGitLabRestApiClient
            .Verify(x => x.ReadGroupAsync(ExpectedGitLabUrl, ExpectedAccessToken, ExpectedGroupId, It.IsAny<CancellationToken>())
                , Times.Once);
        mockGitLabRestApiClient
            .Verify(x => x.ReadAllPipelinesAsync(ExpectedGitLabUrl, ExpectedAccessToken,
                    It.Is<Project>(y => y.Id == ExpectedProjectId), It.IsAny<CancellationToken>())
                , Times.Once);
        mockGitLabRestApiClient
            .Verify(x => x.DeletePipelinesAsync(ExpectedGitLabUrl, ExpectedAccessToken,
                    It.IsAny<Project>(), It.IsAny<Pipeline[]>(), It.IsAny<CancellationToken>())
                , Times.Never);
    }

    [TestMethod]
    public async Task DeleteBuildPipelineAsync_GroupExistsButHasNoProjects_CallExpectedMethods()
    {
        var mockGitLabRestApiClient = new Mock<IGitlabRestApiClient>();
        mockGitLabRestApiClient
            .Setup(x =>
                x.ReadGroupAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Group
            {
                Id = ExpectedGroupId,
                Projects = []
            });
        var sut = CreateSut(mockGitLabRestApiClient.Object);
        var args = CreateArgumentsWithGroupId();
        var result = await sut.DeleteBuildPipelineAsync(args);
        Assert.AreEqual(result, ExitCodeTypes.Ok);
        mockGitLabRestApiClient
            .Verify(x => x.ReadAllPipelinesAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Project>(), It.IsAny<CancellationToken>())
                , Times.Never);
    }

    [TestMethod]
    public async Task DeleteBuildPipelineAsync_GroupDoesNotExist_CallExpectedMethods()
    {
        var mockGitLabRestApiClient = new Mock<IGitlabRestApiClient>();
        mockGitLabRestApiClient
            .Setup(x =>
                x.ReadGroupAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group)null!);
        var sut = CreateSut(mockGitLabRestApiClient.Object);
        var args = CreateArgumentsWithGroupId();
        var result = await sut.DeleteBuildPipelineAsync(args);
        Assert.AreEqual(result, ExitCodeTypes.IllegalArguments);
        mockGitLabRestApiClient
            .Verify(x => x.ReadAllPipelinesAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Project>(), It.IsAny<CancellationToken>())
                , Times.Never);
    }

    [TestMethod]
    public async Task DeleteBuildPipelineAsync_ProjectExists_CallExpectedMethods()
    {
        var mockGitLabRestApiClient = new Mock<IGitlabRestApiClient>();
        mockGitLabRestApiClient
            .Setup(x =>
                x.ReadProjectAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Project
            {
                Id = ExpectedProjectId
            });
        mockGitLabRestApiClient.Setup(x =>
                x.ReadAllPipelinesAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Pipeline
                {
                    Id = ExpectedPipelineId1,
                    CreatedAt = ExpectedPipeline1CreatedAtUtc
                }
            ]);
        var sut = CreateSut(mockGitLabRestApiClient.Object);
        var args = CreateArgumentsWithProjectId();
        var result = await sut.DeleteBuildPipelineAsync(args);
        Assert.AreEqual(result, ExitCodeTypes.Ok);
        mockGitLabRestApiClient
            .Verify(x => x.ReadProjectAsync(ExpectedGitLabUrl, ExpectedAccessToken, ExpectedProjectId, It.IsAny<CancellationToken>())
                , Times.Once);
        mockGitLabRestApiClient
            .Verify(x => x.ReadAllPipelinesAsync(ExpectedGitLabUrl, ExpectedAccessToken,
                    It.Is<Project>(y => y.Id == ExpectedProjectId), It.IsAny<CancellationToken>())
                , Times.Once);
        mockGitLabRestApiClient
            .Verify(x => x.DeletePipelinesAsync(ExpectedGitLabUrl, ExpectedAccessToken,
                    It.Is<Project>(y => y.Id == ExpectedProjectId), It.Is<Pipeline[]>(y => VerifyPipelines(y)), It.IsAny<CancellationToken>())
                , Times.Once);
    }

    [TestMethod]
    public async Task DeleteBuildPipelineAsync_ProjectExists_PipelinesOlderThanInDaysIsSet_DeletePipeline1ButNotPipeline2DueToCreationDate()
    {
        var mockGitLabRestApiClient = new Mock<IGitlabRestApiClient>();
        mockGitLabRestApiClient
            .Setup(x =>
                x.ReadProjectAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Project
            {
                Id = ExpectedProjectId
            });
        mockGitLabRestApiClient.Setup(x =>
                x.ReadAllPipelinesAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Pipeline
                {
                    Id = ExpectedPipelineId2,
                    CreatedAt = ExpectedPipeline2CreatedAtUtc
                },
                new Pipeline
                {
                    Id = ExpectedPipelineId1,
                    CreatedAt = ExpectedPipeline1CreatedAtUtc
                }
            ]);
        var sut = CreateSut(mockGitLabRestApiClient.Object);
        var args = CreateArgumentsWithProjectId();
        args.PipelinesOlderThanInDays = 1;
        var result = await sut.DeleteBuildPipelineAsync(args);
        Assert.AreEqual(result, ExitCodeTypes.Ok);
        mockGitLabRestApiClient
            .Verify(x => x.ReadProjectAsync(ExpectedGitLabUrl, ExpectedAccessToken, ExpectedProjectId, It.IsAny<CancellationToken>())
                , Times.Once);
        mockGitLabRestApiClient
            .Verify(x => x.ReadAllPipelinesAsync(ExpectedGitLabUrl, ExpectedAccessToken,
                    It.Is<Project>(y => y.Id == ExpectedProjectId), It.IsAny<CancellationToken>())
                , Times.Once);
        mockGitLabRestApiClient
            .Verify(x => x.DeletePipelinesAsync(ExpectedGitLabUrl, ExpectedAccessToken,
                    It.Is<Project>(y => y.Id == ExpectedProjectId), It.Is<Pipeline[]>(y => VerifyPipelines(y)), It.IsAny<CancellationToken>())
                , Times.Once);
    }

    private static bool VerifyPipelines(IReadOnlyList<Pipeline> pipelines)
    {
        Assert.AreEqual(1, pipelines.Count);
        var pipeline1 = pipelines[0];
        Assert.IsNotNull(pipeline1);
        Assert.IsNotNull(pipeline1.Id);
        Assert.AreEqual(ExpectedPipelineId1, pipeline1.Id.Value);
        return true;
    }

    [TestMethod]
    public async Task DeleteBuildPipelineAsync_ProjectDoesNotExist_CallExpectedMethods()
    {
        var mockGitLabRestApiClient = new Mock<IGitlabRestApiClient>();
        mockGitLabRestApiClient
            .Setup(x =>
                x.ReadProjectAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project)null!);
        var sut = CreateSut(mockGitLabRestApiClient.Object);
        var args = CreateArgumentsWithProjectId();
        var result = await sut.DeleteBuildPipelineAsync(args);
        Assert.AreEqual(result, ExitCodeTypes.IllegalArguments);
        mockGitLabRestApiClient
            .Verify(x => x.ReadAllPipelinesAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Project>(), It.IsAny<CancellationToken>())
                , Times.Never);
    }

    private static DeleteBuildPipelineArgument CreateArgumentsWithGroupId()
    {
        return new DeleteBuildPipelineArgument
        {
            AccessToken = ExpectedAccessToken,
            GitLabUrl = ExpectedGitLabUrl,
            GroupId = ExpectedGroupId,
            PipelinesToKeep = 0
        };
    }

    private static DeleteBuildPipelineArgument CreateArgumentsWithProjectId()
    {
        return new DeleteBuildPipelineArgument
        {
            AccessToken = ExpectedAccessToken,
            GitLabUrl = ExpectedGitLabUrl,
            ProjectId = ExpectedProjectId,
            PipelinesToKeep = 0
        };
    }

    private static DeleteOldPipelinesService CreateSut(IGitlabRestApiClient gitlabRestApiClient)
    {
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetUtcNow(ExpectedUtcNow);
        var logger = new Mock<ILogger<DeleteOldPipelinesService>>();
        return new DeleteOldPipelinesService(fakeTimeProvider, gitlabRestApiClient, logger.Object);
    }
}
