using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.IO;
using Kernel.FOS_System.IO.Streams;

namespace Kernel.Core.Processes.ELF
{
    public unsafe class ELFStringTable : FOS_System.Object
    {
        protected byte* data;
        protected uint size;
        
        public ELFStringTable(uint anAddress, uint aSize)
        {
            data = (byte*)anAddress;
            size = aSize;
        }
        
        public FOS_System.String this[uint offset]
        {
            get
            {
                FOS_System.String currString = "";
                if (offset < size)
                {
                    currString = ByteConverter.GetASCIIStringFromASCII(data, offset, (uint)(size - offset));
                }
                return currString;
            }
        }
    }
}
