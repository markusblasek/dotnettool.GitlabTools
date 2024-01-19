using Flurl;
using GitLabTools.Commandline;

namespace GitLabTools.Validators;

public static class GitLabInformationArgumentValidator
{
    /// <exception cref="ArgumentValidationException"></exception>
    public static void Validate(IGitLabInformationArgument args)
    {
        if (!Url.IsValid(args.GitLabUrl))
        {
            throw new ArgumentValidationException($"{nameof(args.GitLabUrl)} is not a valid url");
        }
        if (string.IsNullOrWhiteSpace(args.AccessToken))
        {
            throw new ArgumentValidationException($"{nameof(args.AccessToken)} is not set");
        }
    }
}
