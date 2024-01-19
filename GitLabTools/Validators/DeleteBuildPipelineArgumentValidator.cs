using GitLabTools.Commandline;

namespace GitLabTools.Validators;

public static class DeleteBuildPipelineArgumentValidator
{
    /// <exception cref="ArgumentValidationException"></exception>
    public static void Validate(DeleteBuildPipelineArgument args)
    {
        GitLabInformationArgumentValidator.Validate(args);

        var argumentNameProjectId =
            ExpressionUtils.GetCommandlineArgumentLongName(() => args.ProjectId);
        var argumentNameGroupId =
            ExpressionUtils.GetCommandlineArgumentLongName(() => args.GroupId);
        var argumentNamePipelinesToKeep =
            ExpressionUtils.GetCommandlineArgumentLongName(() => args.PipelinesToKeep);

        if (args.ProjectId is < ValidationConstants.ProjectIdMinValue)
        {
            throw new ArgumentValidationException($"{argumentNameProjectId} is not a valid project id");
        }
        if (args.GroupId is < ValidationConstants.GroupIdMinValue)
        {
            throw new ArgumentValidationException($"{argumentNameGroupId} is not a valid group id");
        }
        if (!(args.ProjectId.HasValue ^ args.GroupId.HasValue))
        {
            throw new ArgumentValidationException($"Either {argumentNameProjectId} or {argumentNameGroupId} have to have a valid value");
        }
        if (args.PipelinesToKeep < 0)
        {
            throw new ArgumentValidationException($"{argumentNamePipelinesToKeep} is less than 0");
        }
    }
}
