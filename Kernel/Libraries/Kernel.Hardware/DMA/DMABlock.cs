using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.DMA
{
    public unsafe class DMABlock
    {
        public byte* Base;
        public uint Size;

        public DMABlock(byte* aBase, uint aSize)
        {
            Base = aBase;
            Size = aSize;
        }

        public UInt32 this[UInt32 byteIndex]
        {
            get
            {
                if (byteIndex >= Size)
                {
                    Kernel.ExceptionMethods.Throw(new FOS_System.Exception("DMABlock: Memory access violation! byteIndex >= Size"));
                }
                return *(UInt32*)(Base + byteIndex);
            }
            set
            {
                if (byteIndex >= Size)
                {
                    Kernel.ExceptionMethods.Throw(new FOS_System.Exception("DMABlock: Memory access violation! byteIndex >= Size"));
                }
                *(UInt32*)(Base + byteIndex) = value;
            }
        }
    }
}
