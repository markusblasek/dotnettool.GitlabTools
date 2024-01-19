using System.Diagnostics.CodeAnalysis;

namespace GitLabTools.GitLab;

[ExcludeFromCodeCoverage]
public class GitLabFailedException(string message, Exception ex) : Exception(message, ex);