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

#define ISO9660FileStream_TRACE
#undef ISO9660FileStream_TRACE

using Kernel.FileSystems.ISO9660;
using Kernel.Framework;
using Kernel.Framework.Exceptions;

namespace Kernel.FileSystems.Streams.ISO9660
{
    /// <summary>
    ///     Represents a file stream to a ISO9660 file or ISO9660 directory file.
    /// </summary>
    public class ISO9660FileStream : FileStream
    {
        public bool ActuallyDoRead = true;

        /// <summary>
        ///     The position (as an offset from the start of the file) of the stream in the file.
        /// </summary>
        protected ulong position;

        private byte[] ReadSectorBuffer;
        private uint ReadSectorSize;

        //TODO: This implementation has no way of shrinking files - only growing them!

        /// <summary>
        ///     The ISO9660 file system to which the file the stream is for belongs.
        /// </summary>
        public ISO9660FileSystem TheISO9660FileSystem
        {
            get { return (ISO9660FileSystem)TheFile.TheFileSystem; }
        }

        /// <summary>
        ///     The ISO9660 file the stream is for.
        /// </summary>
        public ISO9660File TheISO9660File
        {
            get { return (ISO9660File)TheFile; }
        }

        /// <summary>
        ///     Gets or sets the position (as an offset from the start of the file) of the stream in the file.
        /// </summary>
        public override long Position
        {
            get { return (long)position; }
            set
            {
                if (value < 0L)
                {
                    ExceptionMethods.Throw(new ArgumentException("ISO9660FileStream.Position value must be > 0!"));
                }
                position = (ulong)value;
            }
        }

        /// <summary>
        ///     Initializes a new ISO9660 file stream for the specified file.
        /// </summary>
        /// <param name="aFile">The file to create a stream to.</param>
        public ISO9660FileStream(ISO9660File aFile)
            : base(aFile)
        {
            if (TheISO9660File == null)
            {
                ExceptionMethods.Throw(
                    new Exception("Could not create ISO9660FileStream. Specified file object was null!"));
            }
        }

        /// <summary>
        ///     Reads the specified number of bytes from the stream from the current position into the buffer at the
        ///     specified offset or as many bytes as are available before the end of the stream is met.
        /// </summary>
        /// <param name="buffer">The byte array to read into.</param>
        /// <param name="offset">The offset within the buffer to start storing read data at.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            //Don't attempt to read a file of 0 size.
            if (TheFile.Size > 0)
            {
                ISO9660FileSystem TheFS = TheISO9660FileSystem;

#if ISO9660FileStream_TRACE
                BasicConsole.WriteLine("Checking params...");
#endif

                //Conditions for being able to read from the stream.
                if (count < 0)
                {
                    ExceptionMethods.Throw(new ArgumentException("ISO9660FileStream.Read: aCount must be > 0"));
                }
                else if (offset < 0)
                {
                    ExceptionMethods.Throw(new ArgumentException("ISO9660FileStream.Read: anOffset must be > 0"));
                }
                else if (buffer == null)
                {
                    ExceptionMethods.Throw(new ArgumentException("ISO9660FileStream.Read: aBuffer must not be null!"));
                }
                else if (buffer.Length - offset < count)
                {
                    ExceptionMethods.Throw(
                        new ArgumentException("ISO9660FileStream.Read: Invalid offset / length values!"));
                }
                else if (position == TheFile.Size)
                {
                    // EOF
                    return 0;
                }

#if ISO9660FileStream_TRACE
                BasicConsole.WriteLine("Params OK.");
#endif

                // Clamp the count value so that no out of bounds exceptions occur
                ulong FileSize = TheFile.Size;
                ulong MaxReadableBytes = FileSize - position;
                ulong ActualCount = (ulong)count;
                if (ActualCount > MaxReadableBytes)
                {
                    ActualCount = MaxReadableBytes;
                }

#if ISO9660FileStream_TRACE
                BasicConsole.WriteLine("Creating new sector array...");
#endif

                //Temporary store of cluster data since we can only
                //  read entire clusters at a time.
                if (ReadSectorBuffer == null)
                {
                    ReadSectorBuffer = TheFS.ThePartition.NewBlockArray(1);
                    ReadSectorSize = (uint)TheFS.ThePartition.BlockSize;

#if ISO9660FileStream_TRACE
                    BasicConsole.WriteLine(((Framework.String)"ReadSectorSize: ") + ReadSectorSize);
#endif
                }

                int read = 0;

#if ISO9660FileStream_TRACE
                BasicConsole.WriteLine("Reading data...");
#endif
                //Loop reading in the data
                while (ActualCount > 0)
                {
                    uint SectorIdx = (uint)position/ReadSectorSize + TheISO9660File.TheDirectoryRecord.LBALocation;
                    uint PosInSector = (uint)position%ReadSectorSize;
#if ISO9660FileStream_TRACE
                    BasicConsole.WriteLine(((Framework.String)"Reading sector ") + SectorIdx);
#endif
                    TheFS.ThePartition.ReadBlock(SectorIdx, 1, ReadSectorBuffer);
#if ISO9660FileStream_TRACE
                    BasicConsole.WriteLine("Read sector.");
#endif
                    uint ReadSize;
                    if (PosInSector + ActualCount > ReadSectorSize)
                    {
                        ReadSize = ReadSectorSize - PosInSector;
                    }
                    else
                    {
                        ReadSize = (uint)ActualCount;
                    }

#if ISO9660FileStream_TRACE
                    BasicConsole.WriteLine("Read sector buffer: ");
                    unsafe
                    {
                        BasicConsole.DumpMemory((byte*)Utilities.ObjectUtilities.GetHandle(ReadSectorBuffer) + Framework.Array.FieldsBytesSize, ReadSectorBuffer.Length);
                    }
#endif
                    // TODO: Should we do an argument check here just in case?
                    Array.Copy(ReadSectorBuffer, (int)PosInSector, buffer, offset, (int)ReadSize);
                    offset += (int)ReadSize;
                    ActualCount -= ReadSize;
                    read += (int)ReadSize;
                    position += ReadSize;
                }

#if ISO9660FileStream_TRACE
                BasicConsole.WriteLine("Read data.");
#endif

                return read;
            }
            return 0;
        }

        /// <summary>
        ///     Writes the specified number of the bytes from the buffer starting at offset in the buffer.
        /// </summary>
        /// <param name="buffer">The data to write.</param>
        /// <param name="offset">The offset within the buffer to start writing from.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            ExceptionMethods.Throw(
                new NotSupportedException("Cannot modify contents of ISO9660 disc (yet)! (FileStream)"));
        }
    }
}