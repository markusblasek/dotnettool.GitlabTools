using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
// ReSharper disable UnusedMember.Global

namespace GitLabTools.GitLab.Models;

[ExcludeFromCodeCoverage]
public class Project
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("name_with_namespace")]
    public string? NameWithNamespace { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("path_with_namespace")]
    public string? PathWithNamespace { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("default_branch")]
    public string? DefaultBranch { get; set; }

    [JsonPropertyName("tag_list")]
    public string[]? TagList { get; set; }

    [JsonPropertyName("topics")]
    public string[]? Topics { get; set; }

    [JsonPropertyName("ssh_url_to_repo")]
    public string? SshUrlToRepo { get; set; }

    [JsonPropertyName("http_url_to_repo")]
    public string? HttpUrlToRepo { get; set; }

    [JsonPropertyName("web_url")]
    public string? WebUrl { get; set; }

    [JsonPropertyName("empty_repo")]
    public bool? EmptyRepo { get; set; }

    [JsonPropertyName("archived")]
    public bool? Archived { get; set; }

    [JsonPropertyName("visibility")]
    public string? Visibility { get; set; }
}
