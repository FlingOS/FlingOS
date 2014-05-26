using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware
{
    public abstract class BlockDevice : Device
    {
        protected UInt64 blockCount = 0;
        public UInt64 BlockCount
        {
            get { return blockCount; }
        }

        protected UInt64 blockSize = 0;
        public UInt64 BlockSize
        {
            get { return blockSize; }
        }

        public abstract void ReadBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData);
        /// <remarks>
        /// If data is null, all data to be written should be assumed to be 0.
        /// </remarks>
        public abstract void WriteBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData);

        public byte[] NewBlockArray(UInt32 aBlockCount)
        {
            //TODO - Err..this cast here is really really bad practice but it's just because 
            //  we can't do 64 bit x 64 bit multiplication nor support the conv.ovf.i.un op
            return new byte[aBlockCount * (UInt32)blockSize];
        }
    }
}
