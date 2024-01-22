using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace GitLabTools.Commandline;

[ExcludeFromCodeCoverage]
[Verb("readGroup", false, ["rg"], HelpText = "Read group information and print it to console as json string (important properties)")]
public class ReadGroupInformationArgument : IGitLabInformationArgument
{
    [Option('g', "groupId", Required = true, HelpText = "Group ID to read information from")]
    public int GroupId { get; set; }

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
            new("Read group information",
                new ReadGroupInformationArgument
                {
                    AccessToken = "<PersonalAccessToken>", GitLabUrl = "https://mygitlabinstance.com", GroupId = 123456
                })
        };
}
