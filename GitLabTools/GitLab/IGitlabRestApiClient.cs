using GitLabTools.GitLab.Models;

namespace GitLabTools.GitLab;

public interface IGitlabRestApiClient
{
    /// <summary>
    /// Reads the project from gitlab by project id or null if no project was found
    /// </summary>
    /// <param name="gitlabUrl"></param>
    /// <param name="accessToken"></param>
    /// <param name="projectId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>the project or null</returns>
    /// <exception cref="GitLabFailedException"></exception>
    public Task<Project?> ReadProjectAsync(string gitlabUrl, string accessToken, int projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the group from gitlab by group id or null if no group was found
    /// </summary>
    /// <param name="gitlabUrl"></param>
    /// <param name="accessToken"></param>
    /// <param name="groupId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>the group or null</returns>
    /// <exception cref="GitLabFailedException"></exception>
    public Task<Group?> ReadGroupAsync(string gitlabUrl, string accessToken, int groupId,
        CancellationToken cancellationToken = default);

    /// <exception cref="GitLabFailedException"></exception>
    public Task<Pipeline[]> ReadAllPipelinesAsync(string gitlabUrl, string accessToken, Project project,
        CancellationToken cancellationToken = default);

    /// <exception cref="GitLabFailedException"></exception>
    public Task DeletePipelinesAsync(string gitlabUrl, string accessToken, Project project,
        Pipeline[] pipelinesToDelete, CancellationToken cancellationToken = default);
}
