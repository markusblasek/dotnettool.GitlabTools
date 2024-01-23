using GitLabTools.Commandline;
using GitLabTools.Validators;

namespace GitlabTools.Tests.Validators;

[TestClass]
public class ReadGroupInformationArgumentValidatorTests
{
    private const int ExpectedGroupId = 1;

    [TestMethod]
    public void Validate_ValidArgumentsWithGroupId_DoNotThrowException()
    {
        var input = CreateValidArguments();

        ReadGroupInformationArgumentValidator.Validate(input);
    }

    [TestMethod]
    [DataRow("")]
    public void Validate_InvalidAccessToken_ThrowException(string accessToken)
    {
        var input = CreateValidArguments();
        input.AccessToken = accessToken;

        Assert.ThrowsException<ArgumentValidationException>(() => ReadGroupInformationArgumentValidator.Validate(input));
    }

    [TestMethod]
    [DataRow(0)]
    public void Validate_InvalidGroupId_ThrowException(int groupId)
    {
        var input = CreateValidArguments();
        input.GroupId = groupId;

        Assert.ThrowsException<ArgumentValidationException>(() => ReadGroupInformationArgumentValidator.Validate(input));
    }

    private static ReadGroupInformationArgument CreateValidArguments()
    {
        return new ReadGroupInformationArgument
        {
            GroupId = ExpectedGroupId,
            AccessToken = "test",
            GitLabUrl = "https://test.blafasel.de"
        };
    }
}