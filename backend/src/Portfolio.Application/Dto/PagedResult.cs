namespace Portfolio.Application.Dto;

/// <summary>Generic paged result for public list endpoints.</summary>
/// <typeparam name="T">The item DTO type.</typeparam>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long TotalCount,
    int TotalPages);
