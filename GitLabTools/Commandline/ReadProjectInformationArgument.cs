using CommandLine;
using CommandLine.Text;
using Flurl;

namespace GitLabTools.Commandline;

[Verb("readProject", false, ["rp"], HelpText = "Read project information")]
public class ReadProjectInformationArgument : IGitlabCiInformationArgument, IValidatableCommandlineArgument
{
    [Option('p', "projectId", Required = true, HelpText = "Project ID to read information from")]
    public int ProjectId { get; set; }

    /// <inheritdoc />
    public string GitLabUrl { get; set; } = string.Empty;

    /// <inheritdoc />
    public string AccessToken { get; set; } = string.Empty;

    /// <inheritdoc />
    public void Validate()
    {
        if (ProjectId <= 0)
        {
            throw new ArgumentValidationException($"{nameof(ProjectId)} is not a valid project id");
        }
        if (!Url.IsValid(GitLabUrl))
        {
            throw new ArgumentValidationException($"{nameof(GitLabUrl)} is not a valid url");
        }
        if (string.IsNullOrWhiteSpace(AccessToken))
        {
            throw new ArgumentValidationException($"{nameof(AccessToken)} is not set");
        }
    }

    // ReSharper disable once StringLiteralTypo
    [Usage(ApplicationAlias = "dotnet gitlabtools")]
    // ReSharper disable once UnusedMember.Global
    public static IEnumerable<Example> Examples =>
        new List<Example>
        {
            new("Read project information",
                new ReadProjectInformationArgument
                {
                    AccessToken = "#PersonalAccessToken#", GitLabUrl = "https://gitlab.test.com", ProjectId = 123456
                })
        };
}
