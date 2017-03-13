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
using Kernel.Framework.Collections;
using Kernel.Utilities;

namespace Kernel.Pipes
{
    /// <summary>
    ///     Represents a pipe. Used only within the core OS.
    /// </summary>
    /// <remarks>
    ///     This class is used only internally in the core OS to manage a pipe.
    /// </remarks>
    public unsafe class Pipe : Object
    {
        /// <summary>
        ///     The internal buffer of the pipe.
        /// </summary>
        private readonly byte[] Buffer;

        private readonly byte* BufferPtr;

        /// <summary>
        ///     The Id of the pipe.
        /// </summary>
        public readonly int Id;

        /// <summary>
        ///     Queue of sizes of data from threads waiting to write to the pipe.
        /// </summary>
        private readonly UInt32Queue SizesWaitingToWrite;

        /// <summary>
        ///     Queue of threads waiting to read from the pipe.
        /// </summary>
        private readonly UInt32Queue ThreadsWaitingToRead;

        /// <summary>
        ///     Queue of threads waiting to write to the pipe.
        /// </summary>
        private readonly UInt32Queue ThreadsWaitingToWrite;

        /// <summary>
        ///     The amount of data available in the pipe.
        /// </summary>
        /// <remarks>
        ///     Must be greater than zero for new reads to be allowed. Auto-reduced when data is read.
        /// </remarks>
        private int DataAvailable;

        /// <summary>
        ///     The offset to read data from in the next read request.
        /// </summary>
        /// <remarks>
        ///     Must be zero for new writes to be allowed. Auto-reset back to zero when DataAvailable hits 0.
        /// </remarks>
        private int DataReadOffset;

        /// <summary>
        ///     The offset to write data to in the next request.
        /// </summary>
        /// <remarks>
        ///     Auto-reset back to zero when DataAvailable hits 0.
        /// </remarks>
        private int DataWriteOffset;

        /// <summary>
        ///     The inpoint of the pipe.
        /// </summary>
        public PipeInpoint Inpoint;

        /// <summary>
        ///     The outpoint of the pipe.
        /// </summary>
        public PipeOutpoint Outpoint;

        public int BufferSize
        {
            get { return Buffer.Length; }
        }

        /// <summary>
        ///     Creates a new pipe.
        /// </summary>
        /// <param name="AnId">The pipe's Id.</param>
        /// <param name="outpoint">The outpoint of the pipe.</param>
        /// <param name="inpoint">The inpoint of the pipe.</param>
        /// <param name="BufferSize">The size of buffer to use within the pipe.</param>
        public Pipe(int AnId, PipeOutpoint outpoint, PipeInpoint inpoint, int BufferSize)
        {
            Id = AnId;
            Outpoint = outpoint;
            Inpoint = inpoint;

            Buffer = new byte[BufferSize];
            BufferPtr = (byte*)ObjectUtilities.GetHandle(Buffer) + Array.FieldsBytesSize;
            DataAvailable = 0;

            ThreadsWaitingToRead = new UInt32Queue(5, true);
            ThreadsWaitingToWrite = new UInt32Queue(5, true);
            SizesWaitingToWrite = new UInt32Queue(5, true);
        }

        /// <summary>
        ///     Whether the pipe can be read (by the caller or a queued thread) at the time of calling.
        /// </summary>
        /// <returns>True if it can be read. Otherwise, false.</returns>
        public bool CanRead()
        {
            return DataAvailable > 0;
        }

        /// <summary>
        ///     Whether the pipe can be written (by the caller or a queued thread) at the time of calling.
        /// </summary>
        /// <returns>True if it can be written. Otherwise, false.</returns>
        public bool CanWrite()
        {
            if (SizesWaitingToWrite.Count > 0)
            {
                return CanWrite((int)SizesWaitingToWrite.Peek());
            }
            return CanWrite(0);
        }

        /// <summary>
        ///     Whether the pipe can be written and whether the specified length of data will fit in the buffer.
        /// </summary>
        /// <param name="length">The length of data to be written.</param>
        /// <returns>True if it can be written and the data will fit in the buffer. Otherwise, false.</returns>
        public bool CanWrite(int length)
        {
            return DataReadOffset == 0 &&
                   length + DataWriteOffset < Buffer.Length;
        }

        /// <summary>
        ///     Reads the pipe into the specified buffer.
        /// </summary>
        /// <param name="outBuffer">The buffer to read data into.</param>
        /// <param name="offset">The offset to start writing into the buffer at.</param>
        /// <param name="length">The maximum length of data to read.</param>
        /// <param name="BytesRead">Out : The actual number of bytes read.</param>
        /// <returns>True if the read was successful. Otherwise, false. Also, false if 0 bytes available.</returns>
        public bool Read(byte* outBuffer, int offset, int length, out int BytesRead)
        {
            if (!CanRead())
            {
                BytesRead = -1;
                return false;
            }

            BytesRead = DataAvailable < length ? DataAvailable : length;
            MemoryUtils.MemCpy(outBuffer + offset, BufferPtr + DataReadOffset, (uint)BytesRead);
            DataReadOffset += BytesRead;
            DataAvailable -= BytesRead;

            if (DataAvailable == 0)
            {
                DataWriteOffset = 0;
                DataReadOffset = 0;
            }

            return true;
        }

        /// <summary>
        ///     Writes the specified buffer into pipe.
        /// </summary>
        /// <param name="inBuffer">The buffer to write data from.</param>
        /// <param name="offset">The offset to start reading from the buffer at.</param>
        /// <param name="length">The exact length of data to write.</param>
        /// <returns>True if the write was successful. Otherwise, false. Also, no partial writes allowed.</returns>
        public bool Write(byte* inBuffer, int offset, int length)
        {
            if (!CanWrite(length))
            {
                return false;
            }

            MemoryUtils.MemCpy(BufferPtr + DataWriteOffset, inBuffer + offset, (uint)length);
            DataWriteOffset += length;

            DataReadOffset = 0;
            DataAvailable += length;

            return true;
        }

        /// <summary>
        ///     Queues the specified thread (of the process owning the inpoint) for a future read from the pipe.
        /// </summary>
        /// <param name="ThreadId">The Id of the thread to queue.</param>
        /// <returns>True if the thread was queued. Otherwise, false.</returns>
        public bool QueueToRead(uint ThreadId)
        {
            ThreadsWaitingToRead.Push(ThreadId);
            return true;
        }

        /// <summary>
        ///     Queues the specified thread (of the process owning the outpoint) for a future write to the pipe.
        /// </summary>
        /// <param name="ThreadId">The Id of the thread to queue.</param>
        /// <returns>True if the thread was queued. Otherwise, false.</returns>
        public bool QueueToWrite(uint ThreadId, int SizeToBeWritten)
        {
            ThreadsWaitingToWrite.Push(ThreadId);
            SizesWaitingToWrite.Push((uint)SizeToBeWritten);
            return true;
        }

        /// <summary>
        ///     Peaks a thread (of the process owning the inpoint) from the read queue.
        /// </summary>
        /// <param name="ThreadId">Out : The peeked thread Id.</param>
        /// <returns>True if an Id was available. Otherwise, false.</returns>
        public bool PeekToRead(out uint ThreadId)
        {
            if (ThreadsWaitingToRead.Count > 0)
            {
                ThreadId = ThreadsWaitingToRead.Peek();
                return true;
            }
            ThreadId = 0;
            return false;
        }

        /// <summary>
        ///     Peaks a thread (of the process owning the outpoint) from the write queue.
        /// </summary>
        /// <param name="ThreadId">Out : The peeked thread Id.</param>
        /// <returns>True if an Id was available. Otherwise, false.</returns>
        public bool PeekToWrite(out uint ThreadId)
        {
            if (ThreadsWaitingToWrite.Count > 0)
            {
                ThreadId = ThreadsWaitingToWrite.Peek();
                return true;
            }
            ThreadId = 0;
            return false;
        }

        /// <summary>
        ///     Dequeues a thread (of the process owning the inpoint) from the read queue.
        /// </summary>
        /// <param name="ThreadId">Out : The dequeued thread Id.</param>
        /// <returns>True if an Id was dequeued. Otherwise, false.</returns>
        public bool DequeueToRead(out uint ThreadId)
        {
            if (ThreadsWaitingToRead.Count > 0)
            {
                ThreadId = ThreadsWaitingToRead.Pop();
                return true;
            }
            ThreadId = 0;
            return false;
        }

        /// <summary>
        ///     Dequeues a thread (of the process owning the outpoint) from the write queue.
        /// </summary>
        /// <remarks>
        ///     Also dequeues from the SizesWaitingToWrite queue.
        /// </remarks>
        /// <param name="ThreadId">Out : The dequeued thread Id.</param>
        /// <returns>True if an Id was dequeued. Otherwise, false.</returns>
        public bool DequeueToWrite(out uint ThreadId)
        {
            if (ThreadsWaitingToWrite.Count > 0)
            {
                ThreadId = ThreadsWaitingToWrite.Pop();
                SizesWaitingToWrite.Pop();
                return true;
            }
            ThreadId = 0;
            return false;
        }

        /// <summary>
        ///     Removes the latest thread from the read queue.
        /// </summary>
        /// <param name="ThreadId">The thread Id which was removed.</param>
        /// <returns>True if a thread Id was removed. Otherwise, false.</returns>
        public bool RemoveLastToRead(out uint ThreadId)
        {
            if (ThreadsWaitingToRead.Count > 0)
            {
                ThreadId = ThreadsWaitingToRead.RemoveLast();
                return true;
            }
            ThreadId = 0;
            return false;
        }

        /// <summary>
        ///     Removes the latest thread from the write queue.
        /// </summary>
        /// <remarks>
        ///     Also removes from the SizesWaitingToWrite queue.
        /// </remarks>
        /// <param name="ThreadId">The thread Id which was removed.</param>
        /// <returns>True if a thread Id was removed. Otherwise, false.</returns>
        public bool RemoveLastToWrite(out uint ThreadId)
        {
            if (ThreadsWaitingToWrite.Count > 0)
            {
                ThreadId = ThreadsWaitingToWrite.RemoveLast();
                SizesWaitingToWrite.RemoveLast();
                return true;
            }
            ThreadId = 0;
            return false;
        }

        /// <summary>
        ///     Determines if any threads are waiting to read.
        /// </summary>
        /// <returns>True if threads are waiting. Otherwise, false.</returns>
        public bool AreThreadsWaitingToRead()
        {
            return ThreadsWaitingToRead.Count > 0;
        }

        /// <summary>
        ///     Determines if any threads are waiting to write.
        /// </summary>
        /// <returns>True if threads are waiting. Otherwise, false.</returns>
        public bool AreThreadsWaitingToWrite()
        {
            return ThreadsWaitingToWrite.Count > 0;
        }
    }
}