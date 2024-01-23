using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace GitLabTools.Commandline;

[ExcludeFromCodeCoverage]
[Verb("readProject", false, ["rp"], HelpText = "Read project information and print it to console as json string (important properties)")]
public class ReadProjectInformationArgument : IGitLabInformationArgument
{
    [Option('p', "projectId", Required = true, HelpText = "Project ID to read information from")]
    public int ProjectId { get; set; }

    /// <inheritdoc />
    public string GitLabUrl { get; set; } = string.Empty;

    /// <inheritdoc />
    public string AccessToken { get; set; } = string.Empty;

    // ReSharper disable once StringLiteralTypo
    [Usage(ApplicationAlias = "dotnet gitlabtools")]
    // ReSharper disable once UnusedMember.Global
    public static IEnumerable<Example> Examples =>
        new List<Example>
        {
            new("Read project information",
                new ReadProjectInformationArgument
                {
                    AccessToken = "<PersonalAccessToken>", GitLabUrl = "https://mygitlabinstance.com", ProjectId = 123456
                })
        };
}
