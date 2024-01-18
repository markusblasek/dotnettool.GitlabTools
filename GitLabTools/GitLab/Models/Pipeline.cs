using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
// ReSharper disable UnusedMember.Global

namespace GitLabTools.GitLab.Models;

[ExcludeFromCodeCoverage]
public class Pipeline
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("http_url_to_repo")]
    public string? HttpUrlToRepo { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdateAt { get; set; }
}
