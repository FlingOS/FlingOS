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
                return 0;
            }

            //TODO

            return 0;
        }
        public bool Write(byte* inBuffer, int offset, int length)
        {
            if (!CanWrite())
            {
                return false;
            }

            //TODO

            return false;
        }
    }

    public struct CreatePipeRequest
    {
        public int BufferSize;
        public PipeClasses Class;
        public PipeSubclasses Subclass;
        public PipeDescriptor Result;
    }
    public struct PipeDescriptor
    {
        public int Id;
    }
}
