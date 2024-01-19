using GitLabTools.Commandline;
using GitLabTools.Validators;

namespace GitlabTools.Tests.Validators;

[TestClass]
public class GitLabInformationArgumentValidatorTests
{
    [TestMethod]
    public void Validate_ValidArguments_DoNotThrowException()
    {
        var input = CreateValidArguments();

        GitLabInformationArgumentValidator.Validate(input);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("https:/dummy.de")]
    public void Validate_InvalidGitLabUrl_ThrowException(string gitLabUrl)
    {
        var input = CreateValidArguments();
        input.GitLabUrl = gitLabUrl;

        Assert.ThrowsException<ArgumentValidationException>(() => GitLabInformationArgumentValidator.Validate(input));
    }

    [TestMethod]
    [DataRow("")]
    public void Validate_InvalidAccessToken_ThrowException(string accessToken)
    {
        var input = CreateValidArguments();
        input.AccessToken = accessToken;

        Assert.ThrowsException<ArgumentValidationException>(() => GitLabInformationArgumentValidator.Validate(input));
    }

    private static UtGitLabInformationArgument CreateValidArguments()
    {
        return new UtGitLabInformationArgument
        {
            AccessToken = "test",
            GitLabUrl = "https://test.blafasel.de"
        };
    }

    private class UtGitLabInformationArgument : IGitLabInformationArgument
    {
        public string GitLabUrl { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }
}