using Portfolio.Application.Common;
using Portfolio.Application.Exceptions;

namespace Portfolio.UnitTests;

/// <summary>
/// Paging is validated in the Application layer so bad values become 422 downstream.
/// </summary>
public sealed class PaginationTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 20)]
    [InlineData(3, 100)]
    public void ValidPaging_ReturnsValues(int page, int pageSize)
    {
        var (actualPage, actualSize) = Pagination.Validate(page, pageSize);

        Assert.Equal(page, actualPage);
        Assert.Equal(pageSize, actualSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Page_BelowOne_ThrowsWithPageError(int page)
    {
        var ex = Assert.Throws<PortfolioValidationException>(() => Pagination.Validate(page, 20));

        Assert.True(ex.Errors.ContainsKey("page"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(101)]
    public void PageSize_OutOfRange_ThrowsWithPageSizeError(int pageSize)
    {
        var ex = Assert.Throws<PortfolioValidationException>(() => Pagination.Validate(1, pageSize));

        Assert.True(ex.Errors.ContainsKey("pageSize"));
    }

    [Fact]
    public void BothInvalid_ReportsBothFields()
    {
        var ex = Assert.Throws<PortfolioValidationException>(() => Pagination.Validate(0, 0));

        Assert.True(ex.Errors.ContainsKey("page"));
        Assert.True(ex.Errors.ContainsKey("pageSize"));
    }
}
