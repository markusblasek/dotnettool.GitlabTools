using System.Diagnostics.CodeAnalysis;

namespace GitLabTools.GitLabRestClient.Models.V4;

[ExcludeFromCodeCoverage]
internal class PaginatedResult<T>
{
    public int PageNum { get; set; }
    public int PagesTotal { get; set; }
    public int PerPage { get; set; }
    public int Total { get; set; }
    public T[]? Data { get; set; } = [];
}
