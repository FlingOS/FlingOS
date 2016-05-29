#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

using Kernel.Framework;

namespace Kernel.FileSystems.Streams
{
    /// <summary>
    ///     Represents any stream.
    /// </summary>
    public abstract class Stream : Object
    {
        /// <summary>
        ///     The current position of the stream as a distance from the start of the stream.
        /// </summary>
        public abstract long Position { get; set; }

        /// <summary>
        ///     Reads the specified number of bytes from the stream from the current position into the buffer at the
        ///     specified offset or as many bytes as are available before the end of the stream is met.
        /// </summary>
        /// <param name="buffer">The byte array to read into.</param>
        /// <param name="offset">The offset within the buffer to start storing read data at.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        public abstract int Read(byte[] buffer, int offset, int count);

        /// <summary>
        ///     Writes the specified number of the bytes from the buffer starting at offset in the buffer.
        /// </summary>
        /// <param name="buffer">The data to write.</param>
        /// <param name="offset">The offset within the buffer to start writing from.</param>
        /// <param name="count">The number of bytes to write.</param>
        public abstract void Write(byte[] buffer, int offset, int count);
    }
}