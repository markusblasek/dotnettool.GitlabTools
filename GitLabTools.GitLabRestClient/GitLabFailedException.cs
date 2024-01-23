using System.Diagnostics.CodeAnalysis;

namespace GitLabTools.GitLabRestClient;

[ExcludeFromCodeCoverage]
public class GitLabFailedException(string message, Exception ex) : Exception(message, ex);