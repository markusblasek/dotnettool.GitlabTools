using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using GitLabTools.Commandline;
using GitLabTools.GitLab;
using Microsoft.Extensions.Logging;

namespace GitLabTools.Services;
public class ReadProjectInformationService(
    ILogger<ReadProjectInformationService> logger,
    IGitlabRestApiClient gitlabRestApiClient)
{
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    /// <summary>
    /// Read project information
    /// </summary>
    /// <param name="args"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="GitlabCiFailedException">will be thrown if an error occured</exception>
    /// <returns></returns>
    public async Task<ExitCodeTypes> ReadProjectInformationAsync(ReadProjectInformationArgument args, CancellationToken cancellationToken = default)
    {
        var project = await gitlabRestApiClient.ReadProjectAsync(args.GitLabUrl, args.AccessToken, args.ProjectId, cancellationToken);
        if (project == null)
        {
            logger.LogWarning("Project with id '{projectId}' could not be found", args.ProjectId);
            return ExitCodeTypes.IllegalArguments;
        }
        logger.LogTrace("Found project with id '{projectId}' (name: '{projectName}', url to repo: '{projectHttpUrlToRepo}')",
            project.Id, project.Name, project.HttpUrlToRepo);
        var projectAsJson = JsonSerializer.Serialize(project, DefaultJsonSerializerOptions);
        logger.LogInformation("{projectAsJson}", projectAsJson);
        return ExitCodeTypes.Ok;
    }
}
