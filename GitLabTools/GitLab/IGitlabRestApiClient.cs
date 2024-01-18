using GitLabTools.GitLab.Models;

namespace GitLabTools.GitLab;

public interface IGitlabRestApiClient
{
    /// <summary>
    /// Reads the project from gitlab or null if no project was found
    /// </summary>
    /// <param name="gitlabUrl"></param>
    /// <param name="accessToken"></param>
    /// <param name="projectId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>the project or null</returns>
    /// <exception cref="GitlabCiFailedException"></exception>
    public Task<Project?> ReadProjectAsync(string gitlabUrl, string accessToken, int projectId,
        CancellationToken cancellationToken = default);

    /// <exception cref="GitlabCiFailedException"></exception>
    public Task<Pipeline[]> ReadAllPipelinesAsync(string gitlabUrl, string accessToken, Project project,
        CancellationToken cancellationToken);

    /// <exception cref="GitlabCiFailedException"></exception>
    public Task DeletePipelinesAsync(string gitlabUrl, string accessToken, Project project,
        Pipeline[] pipelinesToDelete, CancellationToken cancellationToken);
}
