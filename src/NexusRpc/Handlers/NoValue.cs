using System;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Representation of no input/return type for an operation handler. This is the same as
    /// <c>typeof(void)</c> (if not using generics).
    /// </summary>
    public struct NoValue : IEquatable<NoValue>
    {
#pragma warning disable CS1591 // No comment needed
        public static bool operator ==(NoValue left, NoValue right) => true;

        public static bool operator !=(NoValue left, NoValue right) => false;
#pragma warning restore CS1591

        /// <summary>
        /// Normalize the given type if "void"-like (<see cref="NoValue"/>,
        /// <see cref="ValueTuple"/>, or <c>typeof(void)</c>) to <c>typeof(void)</c>.
        /// </summary>
        /// <param name="type">Type to normalize.</param>
        /// <returns><c>typeof(void)</c> if void-like type given, otherwise just the type given.
        /// </returns>
        public static Type NormalizeVoidType(Type type) =>
            IsVoidType(type) ? typeof(void) : type;

        /// <summary>
        /// Whether the given type is "void"-like (<see cref="NoValue"/>,
        /// <see cref="ValueTuple"/>, or <c>typeof(void)</c>).
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>True if void-like type given, false otherwise.</returns>
        public static bool IsVoidType(Type type) =>
            type == typeof(void) || type == typeof(ValueTuple) || type == typeof(NoValue);

        /// <inheritdoc/>
        public bool Equals(NoValue other) => true;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is NoValue;

        /// <inheritdoc/>
        public override int GetHashCode() => 0;
    }
}