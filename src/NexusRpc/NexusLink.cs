using System;

namespace NexusRpc
{
    /// <summary>
    /// A Nexus link.
    /// </summary>
    /// <param name="Uri">URI for the link.</param>
    /// <param name="Type">Type to help decode the link.</param>
    public record NexusLink(Uri Uri, string Type);
}