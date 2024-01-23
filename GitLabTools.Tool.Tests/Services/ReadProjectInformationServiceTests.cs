using GitLabTools.Commandline;
using GitLabTools.GitLabRestClient;
using GitLabTools.GitLabRestClient.Models.V4;
using GitLabTools.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitlabTools.Tests.Services;

[TestClass]
public class ReadProjectInformationServiceTests
{
    private const string ExpectedAccessToken = "accessToken";
    private const string ExpectedGitLabUrl = "https://blafasel.unittest.com";
    private const int ExpectedProjectId = 4711;

    [TestMethod]
    public async Task ReadProjectInformationAsync_CallExpectedMethods()
    {
        var mockGitLabRestApiClient = new Mock<IGitlabRestApiClient>();
        mockGitLabRestApiClient
            .Setup(x =>
                x.ReadProjectAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Project());
        var sut = CreateSut(mockGitLabRestApiClient.Object);
        var args = CreateArguments();
        var result = await sut.ReadProjectInformationAsync(args);
        Assert.AreEqual(result, ExitCodeTypes.Ok);
        mockGitLabRestApiClient
            .Verify(x => x.ReadProjectAsync(ExpectedGitLabUrl, ExpectedAccessToken, ExpectedProjectId, It.IsAny<CancellationToken>())
                , Times.Once);
    }

    [TestMethod]
    public async Task ReadProjectInformationAsync_GroupDoesNotExist_CallExpectedMethods()
    {
        var mockGitLabRestApiClient = new Mock<IGitlabRestApiClient>();
        mockGitLabRestApiClient
            .Setup(x =>
                x.ReadProjectAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project)null!);
        var sut = CreateSut(mockGitLabRestApiClient.Object);
        var args = CreateArguments();
        var result = await sut.ReadProjectInformationAsync(args);
        Assert.AreEqual(result, ExitCodeTypes.IllegalArguments);
    }

    private static ReadProjectInformationArgument CreateArguments()
    {
        return new ReadProjectInformationArgument
        {
            AccessToken = ExpectedAccessToken,
            GitLabUrl = ExpectedGitLabUrl,
            ProjectId = ExpectedProjectId
        };
    }

    private static ReadProjectInformationService CreateSut(IGitlabRestApiClient gitlabRestApiClient)
    {
        var logger = new Mock<ILogger<ReadProjectInformationService>>();
        return new ReadProjectInformationService(gitlabRestApiClient, logger.Object);
    }
}
