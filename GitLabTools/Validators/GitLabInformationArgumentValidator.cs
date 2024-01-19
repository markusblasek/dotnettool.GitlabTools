using Flurl;
using GitLabTools.Commandline;

namespace GitLabTools.Validators;

public static class GitLabInformationArgumentValidator
{
    /// <exception cref="ArgumentValidationException"></exception>
    public static void Validate(IGitLabInformationArgument args)
    {
        var argumentNameGitLabUrl =
            ExpressionUtils.GetCommandlineArgumentLongName(() => args.GitLabUrl);
        var argumentNameAccessToken =
            ExpressionUtils.GetCommandlineArgumentLongName(() => args.AccessToken);

        if (!Url.IsValid(args.GitLabUrl))
        {
            throw new ArgumentValidationException($"{argumentNameGitLabUrl} is not a valid url");
        }
        if (string.IsNullOrWhiteSpace(args.AccessToken))
        {
            throw new ArgumentValidationException($"{argumentNameAccessToken} is not set");
        }
    }
}
