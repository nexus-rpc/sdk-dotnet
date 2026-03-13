namespace NexusRpc.Tests;

using System;
using Xunit;

public class OperationExceptionTests
{
    [Fact]
    public void CreateFailedDefaultsToNull()
    {
        var ex = OperationException.CreateFailed("fail");
        Assert.Equal(OperationState.Failed, ex.State);
        Assert.Null(ex.OriginalFailure);
    }

    [Fact]
    public void CreateCanceledDefaultsToNull()
    {
        var ex = OperationException.CreateCanceled("canceled");
        Assert.Equal(OperationState.Canceled, ex.State);
        Assert.Null(ex.OriginalFailure);
    }

    [Fact]
    public void CreateFailedWithOriginalFailure()
    {
        var failure = new Failure("root cause");
        var ex = OperationException.CreateFailed("fail", originalFailure: failure);
        Assert.Equal(OperationState.Failed, ex.State);
        Assert.Same(failure, ex.OriginalFailure);
    }

    [Fact]
    public void CreateCanceledWithOriginalFailure()
    {
        var failure = new Failure("canceled cause");
        var ex = OperationException.CreateCanceled("canceled", originalFailure: failure);
        Assert.Equal(OperationState.Canceled, ex.State);
        Assert.Same(failure, ex.OriginalFailure);
    }

    [Fact]
    public void CreateFailedWithStackTrace()
    {
        var ex = OperationException.CreateFailed("fail", stackTrace: "at Remote.Method()");
        Assert.Equal("at Remote.Method()", ex.StackTrace);
    }

    [Fact]
    public void CreateCanceledWithStackTrace()
    {
        var ex = OperationException.CreateCanceled("canceled", stackTrace: "at Remote.Method()");
        Assert.Equal("at Remote.Method()", ex.StackTrace);
    }

    [Fact]
    public void StackTraceDefaultsToRuntimeTrace()
    {
        var ex = OperationException.CreateFailed("fail");
        Assert.Null(ex.StackTrace);
    }

    [Fact]
    public void CreateFailedWithInnerExceptionAndAllFields()
    {
        var inner = new InvalidOperationException("inner");
        var failure = new Failure("info");
        var ex = OperationException.CreateFailed(
            "fail",
            innerException: inner,
            stackTrace: "remote trace",
            originalFailure: failure);
        Assert.Equal(OperationState.Failed, ex.State);
        Assert.Same(inner, ex.InnerException);
        Assert.Equal("remote trace", ex.StackTrace);
        Assert.Same(failure, ex.OriginalFailure);
    }

    [Fact]
    public void CreateCanceledWithInnerExceptionAndAllFields()
    {
        var inner = new OperationCanceledException("canceled");
        var failure = new Failure("info");
        var ex = OperationException.CreateCanceled(
            "canceled",
            innerException: inner,
            stackTrace: "remote trace",
            originalFailure: failure);
        Assert.Equal(OperationState.Canceled, ex.State);
        Assert.Same(inner, ex.InnerException);
        Assert.Equal("remote trace", ex.StackTrace);
        Assert.Same(failure, ex.OriginalFailure);
    }
}
