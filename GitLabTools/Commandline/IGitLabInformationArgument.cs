using CommandLine;
// ReSharper disable UnusedMemberInSuper.Global

namespace GitLabTools.Commandline;

public interface IGitLabInformationArgument
{
    [Option('u', "gitLabUrl", Required = true, HelpText = "URL to gitlab instance")]
    public string GitLabUrl { get; set; }

    [Option('a', "accessToken", Required = true, HelpText = "Personal access token for gitlab rest api with scope 'api'")]
    public string AccessToken { get; set; }
}
