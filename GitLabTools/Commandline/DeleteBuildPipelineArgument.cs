using CommandLine;
using CommandLine.Text;
using Flurl;

namespace GitLabTools.Commandline;

[Verb("deletePipelines", false, ["dp"], HelpText = "Delete pipelines")]
public class DeleteBuildPipelineArgument : IGitlabCiInformationArgument, IValidatableCommandlineArgument
{
    [Option('p', "projectId", Required = true, HelpText = "Project ID to delete pipelines from")]
    public int ProjectId { get; set; }

    [Option('k', "pipelinesToKeep", HelpText = "Number of pipelines to keep", Default = 50)]
    public int PipelinesToKeep { get; set; }

    [Option('d', "dryRun", HelpText = "Dry run - no changes will be made")]
    public bool DryRun { get; set; }

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
            new("Delete old pipelines project", 
                new DeleteBuildPipelineArgument
                {
                    AccessToken = "#PersonalAccessToken#", GitLabUrl = "https://gitlab.test.com", 
                    PipelinesToKeep = 80, ProjectId = 123456
                }),
            new("Delete old pipelines project - dry run (no changes are made)",
                new DeleteBuildPipelineArgument
                {
                    AccessToken = "#PersonalAccessToken#", GitLabUrl = "https://gitlab.test.com",
                    PipelinesToKeep = 80, ProjectId = 123456, DryRun = true
                })
        };
}
