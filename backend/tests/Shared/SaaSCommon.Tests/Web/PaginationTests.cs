using SaaSCommon.Web;
using Xunit;

namespace SaaSCommon.Tests.Web;

/// <summary>
/// Unit tests for <see cref="PaginationRequest"/> and <see cref="PagedResult{T}"/>.
/// </summary>
public class PaginationTests
{
    #region PaginationRequest

    [Fact]
    public void PaginationRequest_DefaultValues()
    {
        var request = new PaginationRequest();

        Assert.Equal(1, request.Page);
        Assert.Equal(20, request.PageSize);
    }

    [Fact]
    public void PaginationRequest_CustomValues()
    {
        var request = new PaginationRequest(3, 50);

        Assert.Equal(3, request.Page);
        Assert.Equal(50, request.PageSize);
    }

    #endregion

    #region PagedResult

    [Fact]
    public void PagedResult_TotalPages_CalculatedCorrectly()
    {
        var result = new PagedResult<string>(
            new[] { "a", "b", "c" },
            25,
            1,
            10);

        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void PagedResult_TotalPages_RoundsUp()
    {
        var result = new PagedResult<string>(
            new[] { "a" },
            21,
            1,
            10);

        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void PagedResult_HasPreviousPage_FirstPage_ReturnsFalse()
    {
        var result = new PagedResult<string>(
            Array.Empty<string>(),
            100,
            1,
            20);

        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void PagedResult_HasPreviousPage_SecondPage_ReturnsTrue()
    {
        var result = new PagedResult<string>(
            Array.Empty<string>(),
            100,
            2,
            20);

        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public void PagedResult_HasNextPage_LastPage_ReturnsFalse()
    {
        var result = new PagedResult<string>(
            Array.Empty<string>(),
            40,
            2,
            20);

        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void PagedResult_HasNextPage_NotLastPage_ReturnsTrue()
    {
        var result = new PagedResult<string>(
            Array.Empty<string>(),
            100,
            1,
            20);

        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void PagedResult_Items_AreAccessible()
    {
        var items = new[] { "item1", "item2", "item3" };
        var result = new PagedResult<string>(items, 3, 1, 20);

        Assert.Equal(3, result.Items.Count);
        Assert.Equal("item1", result.Items[0]);
    }

    [Fact]
    public void PagedResult_TotalCount_IsCorrect()
    {
        var result = new PagedResult<int>(
            new[] { 1, 2 },
            100,
            1,
            20);

        Assert.Equal(100, result.TotalCount);
    }

    [Fact]
    public void PagedResult_EmptyItems_HasZeroTotalPages()
    {
        var result = new PagedResult<string>(
            Array.Empty<string>(),
            0,
            1,
            20);

        Assert.Equal(0, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    #endregion
}
