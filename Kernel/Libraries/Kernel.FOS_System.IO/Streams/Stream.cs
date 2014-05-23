using System;

namespace Kernel.FOS_System.IO.Streams
{
    public abstract class Stream : FOS_System.Object
    {
        public abstract long Position
        {
            get;
            set;
        }

        public abstract int Read(byte[] aBuffer, int anOffset, int aCount);
        public abstract void Write(byte[] buffer, int offset, int count);
    }
}
