using GitLabTools.Commandline;

namespace GitLabTools.Validators;

public static class ReadProjectInformationArgumentValidator
{
    /// <exception cref="ArgumentValidationException"></exception>
    public static void Validate(ReadProjectInformationArgument args)
    {
        GitLabInformationArgumentValidator.Validate(args);

        var argumentNameProjectId =
            ExpressionUtils.GetCommandlineArgumentLongName(() => args.ProjectId);

        if (args.ProjectId < ValidationConstants.ProjectIdMinValue)
        {
            throw new ArgumentValidationException($"{argumentNameProjectId} is not a valid project id");
        }
    }
}
