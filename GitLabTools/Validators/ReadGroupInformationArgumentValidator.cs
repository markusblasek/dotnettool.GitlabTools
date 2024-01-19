using GitLabTools.Commandline;

namespace GitLabTools.Validators;

public static class ReadGroupInformationArgumentValidator
{
    /// <exception cref="ArgumentValidationException"></exception>
    public static void Validate(ReadGroupInformationArgument args)
    {
        GitLabInformationArgumentValidator.Validate(args);
        if (args.GroupId < ValidationConstants.GroupIdMinValue)
        {
            throw new ArgumentValidationException($"{nameof(args.GroupId)} is not a valid project id");
        }
    }
}
