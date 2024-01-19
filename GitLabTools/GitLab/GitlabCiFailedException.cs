using System.Diagnostics.CodeAnalysis;

namespace GitLabTools.GitLab;

[ExcludeFromCodeCoverage]
public class GitlabCiFailedException(string message, Exception ex) : Exception(message, ex);