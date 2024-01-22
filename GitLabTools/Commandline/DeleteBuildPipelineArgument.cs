using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace GitLabTools.Commandline;

[ExcludeFromCodeCoverage]
[Verb("deletePipelines", false, ["dp"], HelpText = "Delete pipelines")]
public class DeleteBuildPipelineArgument : IGitLabInformationArgument
{
    [Option('p', "projectId", HelpText = "Project ID to delete pipelines from")]
    public int? ProjectId { get; set; }

    [Option('g', "groupId", HelpText = "Group ID to delete pipelines from")]
    public int? GroupId { get; set; }

    [Option('k', "pipelinesToKeep", HelpText = "Number of pipelines to keep", Default = 50)]
    public int PipelinesToKeep { get; set; }

    [Option('d', "dryRun", HelpText = "Dry run - no changes will be made")]
    public bool DryRun { get; set; }

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
            new("Delete old pipelines in a project", 
                new DeleteBuildPipelineArgument
                {
                    AccessToken = "<PersonalAccessToken>", GitLabUrl = "https://mygitlabinstance.com", 
                    PipelinesToKeep = 80, ProjectId = 123456
                }),
            new("Delete old pipelines in a project - dry run (no changes are made)",
                new DeleteBuildPipelineArgument
                {
                    AccessToken = "<PersonalAccessToken>", GitLabUrl = "https://mygitlabinstance.com",
                    PipelinesToKeep = 80, ProjectId = 123456, DryRun = true
                }),
            new("Delete old pipelines of all projects of a group",
                new DeleteBuildPipelineArgument
                {
                    AccessToken = "<PersonalAccessToken>", GitLabUrl = "https://mygitlabinstance.com",
                    PipelinesToKeep = 80, GroupId = 654321
                })
        };
}
