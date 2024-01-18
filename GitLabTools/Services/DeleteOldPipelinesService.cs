using GitLabTools.Commandline;
using GitLabTools.GitLab;
using Microsoft.Extensions.Logging;

namespace GitLabTools.Services;
public class DeleteOldPipelinesService(
    ILogger<DeleteOldPipelinesService> logger,
    IGitlabRestApiClient gitlabRestApiClient)
{
    /// <summary>
    /// Delete build pipelines
    /// </summary>
    /// <param name="args"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="GitlabCiFailedException">will be thrown if an error occured</exception>
    /// <returns></returns>
    public async Task<ExitCodeTypes> DeleteBuildPipelineAsync(DeleteBuildPipelineArgument args, CancellationToken cancellationToken = default)
    {
        var project = await gitlabRestApiClient.ReadProjectAsync(args.GitLabUrl, args.AccessToken, args.ProjectId, cancellationToken);
        if (args.DryRun)
        {
            logger.LogInformation("Dry run enabled - no changes will be made");
        }
        if (project == null)
        {
            logger.LogWarning("Project with id '{projectId}' could not be found", args.ProjectId);
            return ExitCodeTypes.IllegalArguments;
        }
        logger.LogTrace("Found project with id '{projectId}' (name: '{projectName}', url to repo: '{projectHttpUrlToRepo}')",
            project.Id, project.Name, project.HttpUrlToRepo);
        var pipelines = await gitlabRestApiClient.ReadAllPipelinesAsync(args.GitLabUrl, args.AccessToken, project, cancellationToken);
        var pipelinesToDelete = pipelines.Skip(args.PipelinesToKeep).ToArray();
        logger.LogTrace("{allPipelinesCount} pipelines exist and {pipelinesToDeleteCount} should be deleted for project id '{projectId}' ('{projectName}')",
            pipelines.Length, pipelinesToDelete.Length, project.Id, project.Name);

        if (pipelinesToDelete.Length > 0)
        {
            logger.LogInformation("{count} pipelines to delete for project id '{projectId}' ('{projectName}')",
                pipelinesToDelete.Length, project.Id, project.Name);
            if (args.DryRun)
            {
                return ExitCodeTypes.Ok;
            }
            await gitlabRestApiClient.DeletePipelinesAsync(args.GitLabUrl, args.AccessToken, project,
                pipelinesToDelete, cancellationToken);
            logger.LogInformation("{count} pipelines deleted for project id '{projectId}' ('{projectName}')",
                pipelinesToDelete.Length, project.Id, project.Name);
        }
        else
        {
            logger.LogInformation("{count} pipelines to delete for project id '{projectId}' ('{projectName}'), nothing to do",
                pipelinesToDelete.Length, project.Id, project.Name);
        }

        return ExitCodeTypes.Ok;
    }
}
