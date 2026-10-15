using Portfolio.Application.Exceptions;

namespace Portfolio.Application.Common;

/// <summary>
/// Shared paging validation for public list endpoints.
/// Invalid paging surfaces as 422 (not 400): the values are syntactically valid
/// integers but semantically out of range.
/// </summary>
public static class Pagination
{
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) Validate(int page, int pageSize)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (page < 1)
        {
            errors["page"] = ["Page must be greater than or equal to 1."];
        }

        if (pageSize is < 1 or > MaxPageSize)
        {
            errors["pageSize"] = [$"PageSize must be between 1 and {MaxPageSize}."];
        }

        if (errors.Count > 0)
        {
            throw new PortfolioValidationException(errors);
        }

        return (page, pageSize);
    }
}
