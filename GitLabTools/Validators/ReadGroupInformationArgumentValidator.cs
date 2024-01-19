using GitLabTools.Commandline;

namespace GitLabTools.Validators;

public static class ReadGroupInformationArgumentValidator
{
    /// <exception cref="ArgumentValidationException"></exception>
    public static void Validate(ReadGroupInformationArgument args)
    {
        GitLabInformationArgumentValidator.Validate(args);

        var argumentNameGroupId =
            ExpressionUtils.GetCommandlineArgumentLongName(() => args.GroupId);
        
        if (args.GroupId < ValidationConstants.GroupIdMinValue)
        {
            throw new ArgumentValidationException($"{argumentNameGroupId} is not a valid project id");
        }
    }
}
