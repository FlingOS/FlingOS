using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware;
using Kernel.Hardware.VirtMem;

namespace Kernel.Core.Processes
{
    public class Process : FOS_System.Object
    {
        List Threads = new List();
        MemoryLayout TheMemoryLayout = new MemoryLayout();

        public Process()
        {
        }
    }
}
