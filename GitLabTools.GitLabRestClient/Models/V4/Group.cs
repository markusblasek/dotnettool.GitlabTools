using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

// ReSharper disable UnusedMember.Global

namespace GitLabTools.GitLabRestClient.Models.V4;

[ExcludeFromCodeCoverage]
public class Group
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("web_url")]
    public string? WebUrl { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("parent_id")]
    public int? ParentId { get; set; }

    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    [JsonPropertyName("full_path")]
    public string? FullPath { get; set; }

    [JsonPropertyName("projects")]
    public Project[]? Projects { get; set; }

    [JsonPropertyName("empty_repo")]
    public bool? EmptyRepo { get; set; }

    [JsonPropertyName("archived")]
    public bool? Archived { get; set; }

    [JsonPropertyName("visibility")]
    public string? Visibility { get; set; }
}
