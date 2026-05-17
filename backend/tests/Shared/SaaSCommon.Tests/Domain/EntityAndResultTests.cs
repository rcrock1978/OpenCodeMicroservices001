using Microsoft.Extensions.DependencyInjection;
using SaaSCommon.Domain;
using Xunit;

namespace SaaSCommon.Tests.Domain;

/// <summary>
/// Unit tests for the <see cref="Entity"/> base class.
/// </summary>
public class EntityTests
{
    private class TestEntity : Entity { }

    [Fact]
    public void Entity_Id_IsGenerated()
    {
        var entity = new TestEntity();

        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public void Entity_CreatedAt_IsSet()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var entity = new TestEntity();
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.True(entity.CreatedAt >= before);
        Assert.True(entity.CreatedAt <= after);
    }

    [Fact]
    public void Entity_UpdatedAt_IsSet()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var entity = new TestEntity();
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.True(entity.UpdatedAt >= before);
        Assert.True(entity.UpdatedAt <= after);
    }

    [Fact]
    public void Entity_TenantId_DefaultsToEmpty()
    {
        var entity = new TestEntity();

        Assert.Equal(Guid.Empty, entity.TenantId);
    }

    [Fact]
    public void Entity_DifferentInstances_HaveDifferentIds()
    {
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        Assert.NotEqual(entity1.Id, entity2.Id);
    }
}

/// <summary>
/// Unit tests for the <see cref="Result{T}"/> and <see cref="Result"/> monads.
/// </summary>
public class ResultTests
{
    #region Generic Result<T>

    [Fact]
    public void ResultT_Success_HasValueAndNoError()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void ResultT_Failure_HasErrorAndNoValue()
    {
        var result = Result<int>.Failure("Something went wrong");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(0, result.Value);
        Assert.Equal("Something went wrong", result.Error);
    }

    [Fact]
    public void ResultT_Success_WithReferenceType()
    {
        var obj = new { Name = "Test" };
        var result = Result<object>.Success(obj);

        Assert.True(result.IsSuccess);
        Assert.Same(obj, result.Value);
    }

    [Fact]
    public void ResultT_Failure_WithReferenceType()
    {
        var result = Result<object>.Failure("Not found");

        Assert.True(result.IsFailure);
        Assert.Null(result.Value);
    }

    #endregion

    #region Non-generic Result

    [Fact]
    public void Result_Success_HasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Result_Failure_HasError()
    {
        var result = Result.Failure("Operation failed");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Operation failed", result.Error);
    }

    #endregion

    #region Value Equality (record struct)

    [Fact]
    public void ResultT_EqualResults_AreEqual()
    {
        var r1 = Result<int>.Success(42);
        var r2 = Result<int>.Success(42);

        Assert.Equal(r1, r2);
    }

    [Fact]
    public void ResultT_DifferentResults_AreNotEqual()
    {
        var r1 = Result<int>.Success(42);
        var r2 = Result<int>.Success(99);

        Assert.NotEqual(r1, r2);
    }

    [Fact]
    public void Result_EqualSuccess_AreEqual()
    {
        var r1 = Result.Success();
        var r2 = Result.Success();

        Assert.Equal(r1, r2);
    }

    [Fact]
    public void Result_EqualFailures_AreEqual()
    {
        var r1 = Result.Failure("error");
        var r2 = Result.Failure("error");

        Assert.Equal(r1, r2);
    }

    #endregion
}
