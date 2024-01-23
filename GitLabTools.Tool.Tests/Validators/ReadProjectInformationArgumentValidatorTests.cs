using GitLabTools.Commandline;
using GitLabTools.Validators;

namespace GitlabTools.Tests.Validators;

[TestClass]
public class ReadProjectInformationArgumentValidatorTests
{
    private const int ExpectedProjectId = 1;

    [TestMethod]
    public void Validate_ValidArgumentsWithGroupId_DoNotThrowException()
    {
        var input = CreateValidArguments();

        ReadProjectInformationArgumentValidator.Validate(input);
    }

    [TestMethod]
    [DataRow("")]
    public void Validate_InvalidAccessToken_ThrowException(string accessToken)
    {
        var input = CreateValidArguments();
        input.AccessToken = accessToken;

        Assert.ThrowsException<ArgumentValidationException>(() => ReadProjectInformationArgumentValidator.Validate(input));
    }

    [TestMethod]
    [DataRow(0)]
    public void Validate_InvalidProjectId_ThrowException(int projectId)
    {
        var input = CreateValidArguments();
        input.ProjectId = projectId;

        Assert.ThrowsException<ArgumentValidationException>(() => ReadProjectInformationArgumentValidator.Validate(input));
    }

    private static ReadProjectInformationArgument CreateValidArguments()
    {
        return new ReadProjectInformationArgument
        {
            ProjectId = ExpectedProjectId,
            AccessToken = "test",
            GitLabUrl = "https://test.blafasel.de"
        };
    }
}