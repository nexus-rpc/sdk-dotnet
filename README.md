# Nexus .NET SDK

[![NuGet](https://img.shields.io/nuget/vpre/nexusrpc.svg?style=for-the-badge)](https://www.nuget.org/packages/NexusRpc)
[![MIT](https://img.shields.io/github/license/nexus-rpc/sdk-dotnet.svg?style=for-the-badge)](LICENSE)
[![API Docs](https://img.shields.io/badge/API_docs-blue?style=for-the-badge)](https://nexus-rpc.github.io/sdk-dotnet/)

**⚠️ This SDK is currently at an experimental release stage. Backwards-incompatible changes are anticipated until a
stable release is announced. ⚠️**

.NET SDK for working with [Nexus RPC](https://github.com/nexus-rpc/api). See
[API documentation](https://nexus-rpc.github.io/sdk-dotnet/).

## What is Nexus?

Nexus is a synchronous RPC protocol. Arbitrary length operations are modelled on top of a set of pre-defined synchronous
RPCs.

A Nexus caller calls a handler. The handler may respond inline or return a reference for a future, asynchronous
operation. The caller can cancel an asynchronous operation, check for its outcome, or fetch its current state. The
caller can also specify a callback URL, which the handler uses to asynchronously deliver the result of an operation when
it is ready.

## Installation

Add the `NexusRpc` package from [NuGet](https://www.nuget.org/packages/NexusRpc). For example, using the dotnet CLI:

    dotnet add package NexusRpc

The .NET SDK supports .NET Framework >= 4.6.2, .NET Core >= 3.1 (so includes .NET 5+), and .NET Standard >= 2.0.

## Usage

The SDK currently supports defining Nexus services and implementing handlers for them.

### Defining Services and Operations

Define a Nexus service with an interface containing a `[NexusService]` attribute. Each operation is an interface method
accepting 0 or 1 parameters that has the `[NexusOperation]` attribute. For example:

```csharp
using NexusRpc;

[NexusService]
public interface IGreetingService
{
    [NexusOperation]
    string SayHello1(string name);

    [NexusOperation]
    string SayHello2(string name);
}
```

This can be used by both Nexus callers and Nexus handler implementers. Although not yet implemented in this SDK, clients
can leverage this service for type-safe Nexus invocations. Handlers can be implemented that conform to this service as
seen in the next section.

### Implementing Services and Operations

Nexus service handlers are classes that have the `NexusServiceHandler` attribute and reference the service interface.
Nexus operation handlers are returned via parameterless methods that are effectively "operation handler factories" and
are named the same as the operation. Some shortcuts exist for making operations. For example:

```csharp
using NexusRpc.Handlers;

namespace NexusRpc.Tests.Example;

[NexusServiceHandler(typeof(IGreetingService))]
public class GreetingService(IApiClient ApiClient)
{
    // Can be static
    [NexusOperationHandler]
    public static IOperationHandler<string, string> SayHello1() =>
        // Simple, synchronous operations can use a helper for a lambda
        OperationHandler.Sync<string, string>((ctx, name) => $"Hello, {name}!");

    [NexusOperationHandler]
    public IOperationHandler<string, string> SayHello2() =>
        // Advanced, potentially asynchronous operations can 
        new SayHello2Handler(ApiClient);

    public class  SayHello2Handler(IApiClient ApiClient) : IOperationHandler<string, string>
    {
        public Task<OperationStartResult<string>> StartAsync(
            OperationStartContext context, string name) =>
            throw new NotImplementedException("Excluded for brevity");

        public Task<string> FetchResultAsync(OperationFetchResultContext context) =>
            throw new NotImplementedException("Excluded for brevity");

        public Task<OperationInfo> FetchInfoAsync(OperationFetchInfoContext context) =>
            throw new NotImplementedException("Excluded for brevity");

        public Task CancelAsync(OperationCancelContext context) =>
            throw new NotImplementedException("Excluded for brevity");
    }
}
```

## Development

### Build

Prerequisites:

* [.NET](https://learn.microsoft.com/en-us/dotnet/core/install/)
* This repository cloned

With all prerequisites in place, run:

    dotnet build

Or for release:

    dotnet build --configuration Release

### Code formatting

This project uses StyleCop analyzers with some overrides in `.editorconfig`. To format, run:

    dotnet format

Can also run with `--verify-no-changes` to ensure it is formatted.

### Testing

Run:

    dotnet test

Can add options like:

* `--logger "console;verbosity=detailed"` to show logs
* `--filter "FullyQualifiedName=NexusRpc.Tests.ServiceDefinitionTests.FromType_NonInterface_Bad"` to run a
  specific test

To help show full stdout/stderr, this is also available as an in-proc test program. Run:

    dotnet run --project tests/NexusRpc.Tests

Extra args can be added after `--`, e.g. `-- -verbose` would show verbose logs and `-- --help` would show other
options. If the arguments are anything but `--help`, the current assembly is prepended to the args before sending to the
xUnit runner.