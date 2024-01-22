using System.Diagnostics.CodeAnalysis;

namespace GitLabTools.GitLab.Models;

[ExcludeFromCodeCoverage]
// s. https://docs.gitlab.com/ee/api/commits.html#set-the-pipeline-status-of-a-commit
public static class PipelineStatusConstants
{
    public const string Pending = "pending";
    public const string Running = "running";
    public const string Success = "success";
    public const string Failed = "failed";
    public const string Canceled = "canceled";
}
