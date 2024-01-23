using GitLabTools.Commandline;
using GitLabTools.GitLabRestClient;
using GitLabTools.GitLabRestClient.Models.V4;
using GitLabTools.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitlabTools.Tests.Services;

[TestClass]
public class ReadGroupInformationServiceTests
{
    private const string ExpectedAccessToken = "accessToken";
    private const string ExpectedGitLabUrl = "https://blafasel.unittest.com";
    private const int ExpectedGroupId = 4711;

    [TestMethod]
    public async Task ReadGroupInformationAsync_CallExpectedMethods()
    {
        var mockGitLabRestApiClient = new Mock<IGitlabRestApiClient>();
        mockGitLabRestApiClient
            .Setup(x =>
                x.ReadGroupAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Group());
        var sut = CreateSut(mockGitLabRestApiClient.Object);
        var args = CreateArguments();
        var result = await sut.ReadGroupInformationAsync(args);
        Assert.AreEqual(result, ExitCodeTypes.Ok);
        mockGitLabRestApiClient
            .Verify(x => x.ReadGroupAsync(ExpectedGitLabUrl, ExpectedAccessToken, ExpectedGroupId, It.IsAny<CancellationToken>())
                , Times.Once);
    }

    [TestMethod]
    public async Task ReadGroupInformationAsync_GroupDoesNotExist_CallExpectedMethods()
    {
        var mockGitLabRestApiClient = new Mock<IGitlabRestApiClient>();
        mockGitLabRestApiClient
            .Setup(x =>
                x.ReadGroupAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group)null!);
        var sut = CreateSut(mockGitLabRestApiClient.Object);
        var args = CreateArguments();
        var result = await sut.ReadGroupInformationAsync(args);
        Assert.AreEqual(result, ExitCodeTypes.IllegalArguments);
    }

    private static ReadGroupInformationArgument CreateArguments()
    {
        return new ReadGroupInformationArgument
        {
            AccessToken = ExpectedAccessToken,
            GitLabUrl = ExpectedGitLabUrl,
            GroupId = ExpectedGroupId
        };
    }

    private static ReadGroupInformationService CreateSut(IGitlabRestApiClient gitlabRestApiClient)
    {
        var logger = new Mock<ILogger<ReadGroupInformationService>>();
        return new ReadGroupInformationService(gitlabRestApiClient, logger.Object);
    }
}
