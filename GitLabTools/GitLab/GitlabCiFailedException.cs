namespace GitLabTools.GitLab;
public class GitlabCiFailedException(string message, Exception ex) : Exception(message, ex);