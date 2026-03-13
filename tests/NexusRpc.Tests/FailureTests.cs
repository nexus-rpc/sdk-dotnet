namespace NexusRpc.Tests;

using System.Collections.Generic;
using Xunit;

public class FailureTests
{
    [Fact]
    public void ConstructWithMessageOnly()
    {
        var failure = new Failure("something failed");
        Assert.Equal("something failed", failure.Message);
        Assert.Null(failure.Metadata);
        Assert.Null(failure.Details);
        Assert.Null(failure.StackTrace);
        Assert.Null(failure.Cause);
    }

    [Fact]
    public void ConstructWithAllParameters()
    {
        var metadata = new Dictionary<string, string> { ["key1"] = "val1" };
        var details = new Dictionary<string, object> { ["detail1"] = 42 };
        var cause = new Failure("root cause");
        var failure = new Failure(
            "something failed",
            metadata: metadata,
            details: details,
            stackTrace: "at Foo.Bar()",
            cause: cause);

        Assert.Equal("something failed", failure.Message);
        Assert.Same(metadata, failure.Metadata);
        Assert.Same(details, failure.Details);
        Assert.Equal("at Foo.Bar()", failure.StackTrace);
        Assert.Same(cause, failure.Cause);
    }

    [Fact]
    public void NestedCauseChain()
    {
        var root = new Failure("root");
        var mid = new Failure("mid", cause: root);
        var top = new Failure("top", cause: mid);

        Assert.Equal("mid", top.Cause!.Message);
        Assert.Equal("root", top.Cause!.Cause!.Message);
        Assert.Null(top.Cause!.Cause!.Cause);
    }

    [Fact]
    public void MetadataAndDetailsStoredAsReadOnlyDictionary()
    {
        IReadOnlyDictionary<string, string> metadata = new Dictionary<string, string>
        {
            ["k"] = "v",
        };
        IReadOnlyDictionary<string, object> details = new Dictionary<string, object>
        {
            ["num"] = 1,
            ["str"] = "hello",
        };

        var failure = new Failure("msg", metadata: metadata, details: details);

        Assert.Equal("v", failure.Metadata!["k"]);
        Assert.Equal(1, failure.Details!["num"]);
        Assert.Equal("hello", failure.Details!["str"]);
    }
}
