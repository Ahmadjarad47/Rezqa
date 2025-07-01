using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Common.Models;

public record PaginatedRequest
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    private int _pageNumber = 1;
    public string? search { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    [Range(1, MaxPageSize)]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }

    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}