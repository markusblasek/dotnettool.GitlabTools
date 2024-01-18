using CommandLine;
// ReSharper disable UnusedMemberInSuper.Global

namespace GitLabTools.Commandline;

public interface IGitlabCiInformationArgument
{
    [Option('g', "gitLabUrl", Required = true, HelpText = "URL to gitlab instance")]
    public string GitLabUrl { get; set; }

    [Option('t', "accessToken", Required = true, HelpText = "Personal access token for gitlab ci rest api with scope 'api'")]
    public string AccessToken { get; set; }
}
