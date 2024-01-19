using GitLabTools.Commandline;

namespace GitLabTools.Validators;

public static class ReadProjectInformationArgumentValidator
{
    /// <exception cref="ArgumentValidationException"></exception>
    public static void Validate(ReadProjectInformationArgument args)
    {
        GitLabInformationArgumentValidator.Validate(args);
        if (args.ProjectId < ValidationConstants.ProjectIdMinValue)
        {
            throw new ArgumentValidationException($"{nameof(args.ProjectId)} is not a valid project id");
        }
    }
}
