namespace NexusRpc.Tests;

using System.Collections.Generic;
using Xunit;

public class FailureInfoTests
{
    [Fact]
    public void ConstructWithMessageOnly()
    {
        var failure = new FailureInfo("something failed");
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
        var cause = new FailureInfo("root cause");
        var failure = new FailureInfo(
            "something failed",
            metadata: metadata,
            details: "{\"detail1\": 42}",
            stackTrace: "at Foo.Bar()",
            cause: cause);

        Assert.Equal("something failed", failure.Message);
        Assert.Same(metadata, failure.Metadata);
        Assert.Equal("{\"detail1\": 42}", failure.Details);
        Assert.Equal("at Foo.Bar()", failure.StackTrace);
        Assert.Same(cause, failure.Cause);
    }

    [Fact]
    public void NestedCauseChain()
    {
        var root = new FailureInfo("root");
        var mid = new FailureInfo("mid", cause: root);
        var top = new FailureInfo("top", cause: mid);

        Assert.Equal("mid", top.Cause!.Message);
        Assert.Equal("root", top.Cause!.Cause!.Message);
        Assert.Null(top.Cause!.Cause!.Cause);
    }

    [Fact]
    public void MetadataStoredAsReadOnlyDictionary()
    {
        IReadOnlyDictionary<string, string> metadata = new Dictionary<string, string>
        {
            ["k"] = "v",
        };

        var failure = new FailureInfo("msg", metadata: metadata, details: "some details");

        Assert.Equal("v", failure.Metadata!["k"]);
        Assert.Equal("some details", failure.Details);
    }
}
