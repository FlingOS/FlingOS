using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        
        public Pipe(int AnId, PipeOutpoint outpoint, PipeInpoint inpoint, int BufferSize)
        {
            Id = AnId;
            Outpoint = outpoint;
            Inpoint = inpoint;
            Buffer = new byte[BufferSize];
            DataAvailable = 0;
        }

        public bool CanRead()
        {
            return DataAvailable > 0;
        }
        public bool CanWrite()
        {
            return DataAvailable == 0;
        }

        public int Read(byte* outBuffer, int offset, int length)
        {
            if (!CanRead())
            {
                return -1;
            }

            int BytesRead = 0;
            for (BytesRead = 0; BytesRead < DataAvailable && BytesRead < length; BytesRead++)
            {
                outBuffer[offset++] = Buffer[DataOffset++];
            }

            DataAvailable -= BytesRead;
            
            return BytesRead;
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
    }
    public unsafe struct WritePipeRequest
    {
        public int PipeId;
        public byte* inBuffer;
        public int offset;
        public int length;
    }

    public struct PipeDescriptor
    {
        public int Id;
    }
}
