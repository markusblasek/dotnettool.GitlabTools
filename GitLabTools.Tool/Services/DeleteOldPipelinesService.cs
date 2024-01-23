using GitLabTools.Commandline;
using GitLabTools.GitLabRestClient;
using GitLabTools.GitLabRestClient.Models.V4;
using GitLabTools.Validators;
using Microsoft.Extensions.Logging;

namespace GitLabTools.Services;

public class DeleteOldPipelinesService(
    TimeProvider timeProvider,
    IGitlabRestApiClient gitlabRestApiClient,
    ILogger<DeleteOldPipelinesService> logger)
{
    /// <summary>
    /// Delete build pipelines
    /// </summary>
    /// <param name="args"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="GitLabFailedException">will be thrown if an error occured</exception>
    /// <exception cref="ArgumentValidationException">will be thrown if the arguments are invalid</exception>
    /// <returns></returns>
    public async Task<ExitCodeTypes> DeleteBuildPipelineAsync(DeleteBuildPipelineArgument args, CancellationToken cancellationToken = default)
    {
        DeleteBuildPipelineArgumentValidator.Validate(args);
        if (args.DryRun)
        {
            logger.LogInformation("Dry run enabled - no changes will be made");
        }

        var projects = await GetProjectsToDeletePipelinesFromOrNullAsync(args, cancellationToken);
        if (projects == null)
        {
            return ExitCodeTypes.IllegalArguments;
        }

        await DeletePipelinesFromProjectsAsync(args, projects, cancellationToken);

        return ExitCodeTypes.Ok;
    }

    /// <returns>the projects or null (no project/group found) or an empty list (group has no projects)</returns>
    private async Task<Project[]?> GetProjectsToDeletePipelinesFromOrNullAsync(DeleteBuildPipelineArgument args, CancellationToken cancellationToken)
    {
        if (args.GroupId.HasValue)
        {
            var group = await gitlabRestApiClient.ReadGroupAsync(args.GitLabUrl, args.AccessToken, args.GroupId.Value,
                cancellationToken);
            if (group == null)
            {
                logger.LogWarning("Group with id '{groupId}' could not be found", args.GroupId);
                return null;
            }

            if (group.Projects != null && group.Projects.Length != 0)
            {
                return group.Projects.Where(x => x.Id.HasValue).ToArray();
            }

            logger.LogWarning("Group with id '{groupId}' has no projects", args.GroupId);
            return [];

        }

        if (args.ProjectId.HasValue)
        {
            var project = await gitlabRestApiClient.ReadProjectAsync(args.GitLabUrl, args.AccessToken, args.ProjectId.Value,
                cancellationToken);
            if (project == null)
            {
                logger.LogWarning("Project with id '{projectId}' could not be found", args.ProjectId);
                return null;
            }

            logger.LogTrace(
                "Found project with id '{projectId}' (name: '{projectName}', url to repo: '{projectHttpUrlToRepo}')",
                project.Id, project.Name, project.HttpUrlToRepo);
            return [project];
        }

        throw new InvalidOperationException($"Either {nameof(args.ProjectId)} nor {nameof(args.GroupId)} is set");
    }

    private async Task DeletePipelinesFromProjectsAsync(DeleteBuildPipelineArgument args, IEnumerable<Project> projects, CancellationToken cancellationToken)
    {
        foreach (var project in projects)
        {
            var pipelines =
                (await gitlabRestApiClient.ReadAllPipelinesAsync(args.GitLabUrl, args.AccessToken, project,
                    cancellationToken)).OrderByDescending(x => x.Id).ToArray();
            var pipelinesToDelete = FilterPipelines(pipelines, args);
            logger.LogTrace(
                "{allPipelinesCount} pipelines exist and {pipelinesToDeleteCount} should be deleted for project id '{projectId}' ('{projectName}')",
                pipelines.Length, pipelinesToDelete.Length, project.Id, project.Name);

            if (pipelinesToDelete.Length > 0)
            {
                logger.LogInformation("{countPipelinesToDelete} out of {countAllPipelines} pipelines to delete for project id '{projectId}' ('{projectName}')",
                    pipelinesToDelete.Length, pipelines.Length, project.Id, project.Name);
                if (args.DryRun)
                {
                    continue;
                }
                logger.LogInformation("Deleting {countPipelinesToDelete} pipelines...", pipelinesToDelete.Length);
                await gitlabRestApiClient.DeletePipelinesAsync(args.GitLabUrl, args.AccessToken, project,
                    pipelinesToDelete, cancellationToken);
                logger.LogInformation("{countPipelinesToDelete} pipelines deleted for project id '{projectId}' ('{projectName}')",
                    pipelinesToDelete.Length, project.Id, project.Name);
            }
            else
            {
                logger.LogInformation(
                    "{countPipelinesToDelete} out of {countAllPipelines} pipelines to delete for project id '{projectId}' ('{projectName}'), nothing to do",
                    pipelinesToDelete.Length, pipelines.Length, project.Id, project.Name);
            }
        }
    }

    private Pipeline[] FilterPipelines(IEnumerable<Pipeline> pipelines, DeleteBuildPipelineArgument args)
    {
        var query = pipelines
            .Where(x => !string.IsNullOrWhiteSpace(x.Status) && 
                        (x.Status.Equals(PipelineStatusConstants.Success, StringComparison.InvariantCultureIgnoreCase)
                         || x.Status.Equals(PipelineStatusConstants.Failed, StringComparison.InvariantCultureIgnoreCase)
                         || x.Status.Equals(PipelineStatusConstants.Canceled, StringComparison.InvariantCultureIgnoreCase)))
            .AsQueryable();

        if (args.PipelinesToKeep.HasValue)
        {
            query = query.Skip(args.PipelinesToKeep.Value);
        }

        if (args.PipelinesOlderThanInDays.HasValue)
        {
            var utcNow = timeProvider.GetUtcNow();
            var createdAtFromValue = utcNow.Subtract(TimeSpan.FromDays(args.PipelinesOlderThanInDays.Value));
            query = query.Where(x => x.CreatedAt.ToUniversalTime() < createdAtFromValue);
        }

        return query.OrderBy(x => x.Id).ToArray();
    }
}
