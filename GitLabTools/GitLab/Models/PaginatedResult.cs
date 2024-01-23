namespace GitLabTools.GitLab.Models;

internal class PaginatedResult<T>
{
    public int PageNum { get; set; }
    public int PagesTotal { get; set; }
    public int PerPage { get; set; }
    public int Total { get; set; }
    public T[]? Data { get; set; } = [];
}
