using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Core.Pipes
{
    public unsafe class Pipe : FOS_System.Object
    {
        public int Id;

        public PipeOutpoint Outpoint;
        public PipeInpoint Inpoint;

        private byte[] Buffer;
        private int DataAvailable;
        private int DataOffset;

        private UInt32Queue ThreadsWaitingToWrite;
        private UInt32Queue ThreadsWaitingToRead;

        public Pipe(int AnId, PipeOutpoint outpoint, PipeInpoint inpoint, int BufferSize)
        {
            Id = AnId;
            Outpoint = outpoint;
            Inpoint = inpoint;

            Buffer = new byte[BufferSize];
            DataAvailable = 0;

            ThreadsWaitingToRead = new UInt32Queue(5, true);
            ThreadsWaitingToWrite = new UInt32Queue(5, true);
        }

        public bool CanRead()
        {
            return DataAvailable > 0;
        }
        public bool CanWrite()
        {
            return DataAvailable == 0;
        }

        public bool Read(byte* outBuffer, int offset, int length, out int BytesRead)
        {
            if (!CanRead())
            {
                BytesRead = -1;
                return false;
            }

            BytesRead = 0;
            for (BytesRead = 0; BytesRead < DataAvailable && BytesRead < length; BytesRead++)
            {
                outBuffer[offset++] = Buffer[DataOffset++];
            }

            DataAvailable -= BytesRead;

            return true;
        }
        public bool Write(byte* inBuffer, int offset, int length)
        {
            if (!CanWrite())
            {
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                Buffer[i] = inBuffer[i + offset];
            }

            DataOffset = 0;
            DataAvailable = length;

            return true;
        }

        public bool QueueToRead(UInt32 ThreadId)
        {
            ThreadsWaitingToRead.Push(ThreadId);
            return true;
        }
        public bool QueueToWrite(UInt32 ThreadId)
        {
            ThreadsWaitingToWrite.Push(ThreadId);
            return true;
        }
        public bool DequeueToRead(out UInt32 ThreadId)
        {
            if (ThreadsWaitingToRead.Count > 0)
            {
                ThreadId = ThreadsWaitingToRead.Pop();
                return true;
            }
            else
            {
                ThreadId = 0;
                return false;
            }
        }
        public bool DequeueToWrite(out UInt32 ThreadId)
        {
            if (ThreadsWaitingToWrite.Count > 0)
            {
                ThreadId = ThreadsWaitingToWrite.Pop();
                return true;
            }
            else
            {
                ThreadId = 0;
                return false;
            }
        }
        public bool AreThreadsWaitingToRead()
        {
            return ThreadsWaitingToRead.Count > 0;
        }
        public bool AreThreadsWaitingToWrite()
        {
            return ThreadsWaitingToWrite.Count > 0;
        }
    }

    public struct CreatePipeRequest
    {
        public int BufferSize;
        public PipeClasses Class;
        public PipeSubclasses Subclass;
        public PipeDescriptor Result;
    }
    public unsafe struct ReadPipeRequest
    {
        public int PipeId;
        public byte* outBuffer;
        public int offset;
        public int length;
        public bool blocking;
    }
    public unsafe struct WritePipeRequest
    {
        public int PipeId;
        public byte* inBuffer;
        public int offset;
        public int length;
        public bool blocking;
    }

    public struct PipeDescriptor
    {
        public int Id;
    }
}
