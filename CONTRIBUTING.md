# Contributing to Nexus .NET SDK

See [Development](README.md#development) for build, formatting, and test instructions.

## Release workflows

| File | Name | Purpose |
| --- | --- | --- |
| `.github/workflows/nuget-package.yml` | NuGet Package | Build packages and symbols, then smoke-test the package on Linux, macOS, and Windows. |
| `.github/workflows/nuget-publish.yml` | NuGet Publish | Reusable Trusted Publishing step and package-availability check. |
| `.github/workflows/release-publish.yml` | Release Publish | Validate, publish to integration, smoke-test, publish to production, smoke-test, and create a draft GitHub release. |

## Preparing a release

1. Update `Version` and `AssemblyVersion` in `Directory.Build.props` through a PR.
   Use `major.minor.patch[-prerelease]` without build metadata. The assembly version
   must have the same numeric components; prereleases require a positive fourth
   component, incremented for each prerelease and the final release after it.
2. Run `dotnet test`, `dotnet pack src/NexusRpc -c Release`, and
   `dotnet run --file scripts/release.cs -- test`. Merge the preparation PR
   after CI passes.

## Publishing a release

1. Dispatch **Release Publish** from `main` at the intended release commit.
   Preflight rejects existing tags or package versions in either gallery, including
   unlisted packages. Network and gallery errors fail preflight.
2. Check integration-gallery symbol validation before approving production.
   Both galleries receive the same verified packages and symbols.
3. Review the draft GitHub release's generated notes, target commit, version, and
   prerelease status, and check production symbol validation before publishing the
   GitHub release. Package availability checks do not verify symbol indexing.

The package smoke test restores the exact version into a fresh private cache and
exercises service-definition reflection. It runs on .NET 10; this port does not add
a .NET Framework runtime smoke test or change the library's supported targets.

## Recovery

Use **Re-run failed jobs** on the original run after a transient failure. Retain
the `release-package` artifact: downstream retries must use those same verified
bytes. Runtime packages and symbols are pushed separately so a symbol upload can
be retried after its runtime package already exists.

Do not use **Re-run all jobs** or dispatch another release run for a partially
published version: preflight will reject it. If the original artifacts or successful
upstream jobs are unavailable, reconcile the galleries manually rather than
rebuilding that version. Publishing packages is irreversible; deleting the draft
GitHub release does not undo it. Do not move release tags or overwrite packages.

The workflow serializes release runs, but preflight does not reserve a NuGet
version. Avoid independent uploads while a release is running.

## Trusted Publishing setup

Create `nugetint` and `nugetprod` GitHub environments in `nexus-rpc/sdk-dotnet`.
Restrict deployments to `main` and require approval, particularly for production.
Prevent self-approval and administrator bypass where supported.

Set `NUGET_USER` as a GitHub environment variable or repository Actions variable.
Use the NuGet login profile authorized to publish `NexusRpc`, not an email address.
Use environment-specific values if the galleries use different profiles.

Configure Trusted Publishing policies for `NexusRpc` on both galleries:

| Setting | Integration | Production |
| --- | --- | --- |
| Repository owner | `nexus-rpc` | `nexus-rpc` |
| Repository | `sdk-dotnet` | `sdk-dotnet` |
| Workflow file | `nuget-publish.yml` | `nuget-publish.yml` |
| Environment | `nugetint` | `nugetprod` |

Use the reusable workflow filename **`nuget-publish.yml`**, not its caller
`release-publish.yml`: NuGet validates the publishing job's `job_workflow_ref`.
Do not leave the policy's environment field empty. No stored NuGet API key is needed.
See [NuGet Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).
