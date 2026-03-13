namespace NexusRpc.Tests.Handlers;

using System;
using NexusRpc.Handlers;
using Xunit;

public class HandlerExceptionTests
{
    [Fact]
    public void ExistingEnumConstructorDefaultsToNull()
    {
        var ex = new HandlerException(HandlerErrorType.Internal, "fail");
        Assert.Null(ex.OriginalFailure);
    }

    [Fact]
    public void ExistingStringConstructorDefaultsToNull()
    {
        var ex = new HandlerException("INTERNAL", "fail", null);
        Assert.Null(ex.OriginalFailure);
    }

    [Fact]
    public void EnumConstructorSetsOriginalFailure()
    {
        var failure = new Failure("root cause");
        var ex = new HandlerException(
            HandlerErrorType.Internal,
            "fail",
            null,
            originalFailure: failure);
        Assert.Same(failure, ex.OriginalFailure);
    }

    [Fact]
    public void StringConstructorSetsOriginalFailure()
    {
        var failure = new Failure("root cause");
        var ex = new HandlerException(
            "BAD_REQUEST",
            "fail",
            null,
            originalFailure: failure);
        Assert.Same(failure, ex.OriginalFailure);
    }

    [Fact]
    public void StackTraceOverrideReturnsProvidedValue()
    {
        var ex = new HandlerException(
            HandlerErrorType.Internal,
            "fail",
            null,
            stackTrace: "remote stack trace line 1\nline 2");
        Assert.Equal("remote stack trace line 1\nline 2", ex.StackTrace);
    }

    [Fact]
    public void StackTraceDefaultsToRuntimeTrace()
    {
        var ex = new HandlerException(HandlerErrorType.Internal, "fail");
        // Not thrown, so runtime stack trace is null
        Assert.Null(ex.StackTrace);
    }

    [Fact]
    public void StackTraceOverrideWithStringConstructor()
    {
        var ex = new HandlerException(
            "INTERNAL",
            "fail",
            null,
            stackTrace: "at Remote.Method()");
        Assert.Equal("at Remote.Method()", ex.StackTrace);
    }

    [Fact]
    public void StackTraceOverrideReturnedWhenCastToException()
    {
        var ex = new HandlerException(
            HandlerErrorType.Internal,
            "fail",
            null,
            stackTrace: "remote trace");
        Exception baseEx = ex;
        Assert.Equal("remote trace", baseEx.StackTrace);
    }

    [Fact]
    public void ErrorTypeAndRetryBehaviorUnchanged()
    {
        var ex = new HandlerException(
            HandlerErrorType.BadRequest,
            "bad",
            null,
            originalFailure: new Failure("info"));
        Assert.Equal(HandlerErrorType.BadRequest, ex.ErrorType);
        Assert.Equal("BAD_REQUEST", ex.RawErrorType);
        Assert.False(ex.IsRetryable);
    }

    [Fact]
    public void UnknownStringErrorTypeIsRetryable()
    {
        var ex = new HandlerException("CUSTOM_ERROR", "custom", null);
        Assert.Equal(HandlerErrorType.Unknown, ex.ErrorType);
        Assert.Equal("CUSTOM_ERROR", ex.RawErrorType);
        Assert.True(ex.IsRetryable);
    }
}
