using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace GitLabTools.Commandline;

[ExcludeFromCodeCoverage]
[Verb("deletePipelines", false, ["dp"], HelpText = "Delete pipelines")]
public class DeleteBuildPipelineArgument : IGitLabInformationArgument
{
    public const int PipelinesToKeepDefaultValue = 50;

    [Option('p', "projectId", HelpText = "Project ID to delete pipelines from")]
    public int? ProjectId { get; set; }

    [Option('g', "groupId", HelpText = "Group ID to delete pipelines from")]
    public int? GroupId { get; set; }

    [Option('k', "pipelinesToKeep", Default = PipelinesToKeepDefaultValue, HelpText = "Number of pipelines to keep")]
    public int? PipelinesToKeep { get; set; } = PipelinesToKeepDefaultValue;

    [Option('o', "pipelinesOlderThanInDays", 
        HelpText =
            "Delete pipelines which are older than entered value in days (the creation date time of the pipeline is used)")]
    public int? PipelinesOlderThanInDays { get; set; }

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
            new("Delete old pipelines of a project but keep at least 80 pipelines", 
                new DeleteBuildPipelineArgument
                {
                    AccessToken = "<PersonalAccessToken>", GitLabUrl = "https://mygitlabinstance.com", 
                    PipelinesToKeep = 80, ProjectId = 123456
                }),
            new("Delete old pipelines of a project which are older than 60 days but keep at least 80 pipelines",
                new DeleteBuildPipelineArgument
                {
                    AccessToken = "<PersonalAccessToken>", GitLabUrl = "https://mygitlabinstance.com",
                    PipelinesToKeep = 80, PipelinesOlderThanInDays = 60, ProjectId = 123456
                }),
            new("Delete old pipelines of a project - dry run (no changes are made)",
                new DeleteBuildPipelineArgument
                {
                    AccessToken = "<PersonalAccessToken>", GitLabUrl = "https://mygitlabinstance.com",
                    PipelinesToKeep = 80, ProjectId = 123456, DryRun = true
                }),
            new("Delete old pipelines of all projects of a group but keep 80 pipelines",
                new DeleteBuildPipelineArgument
                {
                    AccessToken = "<PersonalAccessToken>", GitLabUrl = "https://mygitlabinstance.com",
                    PipelinesToKeep = 80, GroupId = 654321
                }),
            new("Delete old pipelines of all projects of a group which are older than 60 days but keep at least 80 pipelines (of each project)",
                new DeleteBuildPipelineArgument
                {
                    AccessToken = "<PersonalAccessToken>", GitLabUrl = "https://mygitlabinstance.com",
                    PipelinesToKeep = 80, PipelinesOlderThanInDays = 60, GroupId = 654321
                })
        };
}
