using GitLabTools.Commandline;

namespace GitLabTools.Validators;

public static class DeleteBuildPipelineArgumentValidator
{
    /// <exception cref="ArgumentValidationException"></exception>
    public static void Validate(DeleteBuildPipelineArgument args)
    {
        GitLabInformationArgumentValidator.Validate(args);
        if (args.ProjectId is < ValidationConstants.ProjectIdMinValue)
        {
            throw new ArgumentValidationException($"{nameof(args.ProjectId)} is not a valid project id");
        }
        if (args.GroupId is < ValidationConstants.GroupIdMinValue)
        {
            throw new ArgumentValidationException($"{nameof(args.ProjectId)} is not a valid group id");
        }
        if (!(args.ProjectId.HasValue ^ args.GroupId.HasValue))
        {
            throw new ArgumentValidationException($"Either {nameof(args.ProjectId)} or {nameof(args.GroupId)} have to have a valid value");
        }
        if (args.PipelinesToKeep < 0)
        {
            throw new ArgumentValidationException($"{nameof(args.PipelinesToKeep)} is less than 0");
        }
    }
}
