using GitLabTools.Commandline;
using GitLabTools.Validators;

namespace GitlabTools.Tests.Validators;

[TestClass]
public class DeleteBuildPipelineArgumentValidatorTests
{
    private const int ExpectedGroupId = 4711;
    private const int ExpectedProjectId = 1147;

    [TestMethod]
    public void Validate_ValidArgumentsWithGroupId_DoNotThrowException()
    {
        var input = CreateValidArguments();
        input.GroupId = ExpectedGroupId;
        input.ProjectId = null;

        DeleteBuildPipelineArgumentValidator.Validate(input);
    }

    [TestMethod]
    public void Validate_ValidArgumentsWithProjectId_DoNotThrowException()
    {
        var input = CreateValidArguments();
        input.GroupId = null;
        input.ProjectId = ExpectedProjectId;

        DeleteBuildPipelineArgumentValidator.Validate(input);
    }

    [TestMethod]
    public void Validate_ProjectIdAndGroupIdAreNotSet_ThrowException()
    {
        var input = CreateValidArguments();
        input.GroupId = null;
        input.ProjectId = null;

        Assert.ThrowsException<ArgumentValidationException>(() => DeleteBuildPipelineArgumentValidator.Validate(input));
    }

    [TestMethod]
    public void Validate_ProjectIdAndGroupIdAreSet_ThrowException()
    {
        var input = CreateValidArguments();
        input.GroupId = ExpectedGroupId;
        input.ProjectId = ExpectedProjectId;

        Assert.ThrowsException<ArgumentValidationException>(() => DeleteBuildPipelineArgumentValidator.Validate(input));
    }

    [TestMethod]
    [DataRow("")]
    public void Validate_InvalidAccessToken_ThrowException(string accessToken)
    {
        var input = CreateValidArguments();
        input.AccessToken = accessToken;

        Assert.ThrowsException<ArgumentValidationException>(() => DeleteBuildPipelineArgumentValidator.Validate(input));
    }

    [TestMethod]
    [DataRow(0)]
    public void Validate_InvalidProjectId_ThrowException(int projectId)
    {
        var input = CreateValidArguments();
        input.ProjectId = projectId;

        Assert.ThrowsException<ArgumentValidationException>(() => DeleteBuildPipelineArgumentValidator.Validate(input));
    }

    [TestMethod]
    [DataRow(0)]
    public void Validate_InvalidGroupId_ThrowException(int groupId)
    {
        var input = CreateValidArguments();
        input.GroupId = groupId;

        Assert.ThrowsException<ArgumentValidationException>(() => DeleteBuildPipelineArgumentValidator.Validate(input));
    }

    [TestMethod]
    [DataRow(-1)]
    public void Validate_InvalidPipelinesToKeepValue_ThrowException(int pipelinesToKeep)
    {
        var input = CreateValidArguments();
        input.PipelinesToKeep = pipelinesToKeep;

        Assert.ThrowsException<ArgumentValidationException>(() => DeleteBuildPipelineArgumentValidator.Validate(input));
    }

    [TestMethod]
    [DataRow(-1)]
    public void Validate_InvalidPipelinesOlderThanInDays_ThrowException(int pipelinesOlderThanInDays)
    {
        var input = CreateValidArguments();
        input.PipelinesOlderThanInDays = pipelinesOlderThanInDays;

        Assert.ThrowsException<ArgumentValidationException>(() => DeleteBuildPipelineArgumentValidator.Validate(input));
    }

    private static DeleteBuildPipelineArgument CreateValidArguments()
    {
        return new DeleteBuildPipelineArgument
        {
            GroupId = ExpectedGroupId,
            PipelinesToKeep = 0,
            AccessToken = "test",
            GitLabUrl = "https://test.blafasel.de"
        };
    }
}