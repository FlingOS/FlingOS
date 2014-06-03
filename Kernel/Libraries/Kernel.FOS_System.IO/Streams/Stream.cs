using System;

namespace Kernel.FOS_System.IO.Streams
{
    /// <summary>
    /// Represents any stream.
    /// </summary>
    public abstract class Stream : FOS_System.Object
    {
        /// <summary>
        /// The current position of the stream as a distance from the start of the stream.
        /// </summary>
        public abstract long Position
        {
            get;
            set;
        }

        /// <summary>
        /// Reads the specified number of bytes from the stream from the current position into the buffer at the 
        /// specified offset or as many bytes as are available before the end of the stream is met.
        /// </summary>
        /// <param name="buffer">The byte array to read into.</param>
        /// <param name="offset">The offset within the buffer to start storing read data at.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        public abstract int Read(byte[] buffer, int offset, int count);
        /// <summary>
        /// Writes the specified number of the bytes from the buffer starting at offset in the buffer.
        /// </summary>
        /// <param name="buffer">The data to write.</param>
        /// <param name="offset">The offset within the buffer to start writing from.</param>
        /// <param name="count">The number of bytes to write.</param>
        public abstract void Write(byte[] buffer, int offset, int count);
    }
}
