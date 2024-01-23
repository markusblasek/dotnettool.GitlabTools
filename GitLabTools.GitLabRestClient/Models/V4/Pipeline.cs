using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

// ReSharper disable UnusedMember.Global

namespace GitLabTools.GitLabRestClient.Models.V4;

[ExcludeFromCodeCoverage]
public class Pipeline
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("iid")]
    public int? Iid { get; set; }

    [JsonPropertyName("project_id")]
    public int? ProjectId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("web_url")]
    public string? WebUrl { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdateAt { get; set; }
}
