using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using GitLabTools.Commandline;
using GitLabTools.GitLabRestClient;
using GitLabTools.Validators;
using Microsoft.Extensions.Logging;

namespace GitLabTools.Services;

public class ReadGroupInformationService(
    IGitlabRestApiClient gitlabRestApiClient,
    ILogger<ReadGroupInformationService> logger)
{
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    /// <summary>
    /// Read group information
    /// </summary>
    /// <param name="args"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="GitLabFailedException">will be thrown if an error occured</exception>
    /// <exception cref="ArgumentValidationException">will be thrown if the arguments are invalid</exception>
    /// <returns></returns>
    public async Task<ExitCodeTypes> ReadGroupInformationAsync(ReadGroupInformationArgument args, CancellationToken cancellationToken = default)
    {
        ReadGroupInformationArgumentValidator.Validate(args);
        var group = await gitlabRestApiClient.ReadGroupAsync(args.GitLabUrl, args.AccessToken, args.GroupId, cancellationToken);
        if (group == null)
        {
            logger.LogWarning("Group with id '{groupId}' could not be found", args.GroupId);
            return ExitCodeTypes.IllegalArguments;
        }
        logger.LogTrace("Found group with id '{groupId}' (name: '{groupName}', url: '{groupWebUrl}')",
            group.Id, group.Name, group.WebUrl);
        var groupAsJson = JsonSerializer.Serialize(group, DefaultJsonSerializerOptions);
        Console.WriteLine(groupAsJson);
        return ExitCodeTypes.Ok;
    }
}
