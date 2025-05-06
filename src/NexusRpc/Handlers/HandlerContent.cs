using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Content that can be used in a handler. Can be streaming or fixed set of bytes.
    /// </summary>
    public class HandlerContent
    {
        private Stream? stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerContent"/> class from a fixed set of
        /// bytes.
        /// </summary>
        /// <param name="bytes">Byte array.</param>
        /// <param name="headers">Headers if any.</param>
        public HandlerContent(
            byte[] bytes,
            IReadOnlyDictionary<string, string>? headers = null)
            // Can add optimization to not create memory stream if needed
            : this(new MemoryStream(bytes), headers)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerContent"/> class from a stream.
        /// </summary>
        /// <param name="stream">Byte stream.</param>
        /// <param name="headers">Headers if any.</param>
        public HandlerContent(
            Stream stream,
            IReadOnlyDictionary<string, string>? headers = null)
        {
            this.stream = stream;
            Headers = headers;
        }

        /// <summary>
        /// Gets the headers on this content if any.
        /// </summary>
        public IReadOnlyDictionary<string, string>? Headers { get; private init; }

        /// <summary>
        /// Consume all bytes as a byte array. Once this has been called, it cannot be called again.
        /// </summary>
        /// <returns>All bytes.</returns>
        /// <exception cref="InvalidOperationException">Already consumed.</exception>
        public byte[] ConsumeAllBytes()
        {
            var stream = ConsumeStream();
            if (stream is MemoryStream memStream)
            {
                return memStream.ToArray();
            }
            using (stream)
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Take the stream off this content for consumption. Once this has been called, it cannot
        /// be called again.
        /// </summary>
        /// <returns>Stream.</returns>
        /// <exception cref="InvalidOperationException">Already consumed.</exception>
        public Stream ConsumeStream() =>
            Interlocked.Exchange(ref stream, null) ??
                throw new InvalidOperationException("Data already consumed");
    }
}