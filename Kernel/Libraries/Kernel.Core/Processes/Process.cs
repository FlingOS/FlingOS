using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware;
using Kernel.Hardware.VirtMem;
using Kernel.FOS_System.IO;

namespace Kernel.Core.Processes
{
    [Compiler.PluggedClass]
    public unsafe class Process : FOS_System.Object
    {
        public List Threads = new List();
        public MemoryLayout TheMemoryLayout = new MemoryLayout();

        public uint Id;
        public FOS_System.String Name;

        private uint ThreadIdGenerator = 0;

        public Process(void* MainMethodPtr, uint AnId, FOS_System.String AName)
        {
            BasicConsole.WriteLine(" > > Constructing process object...");
            Id = AnId;
            Name = AName;

            BasicConsole.WriteLine(" > > Creating thread...");
            Thread mainThread = new Thread(MainMethodPtr, ThreadIdGenerator++);
            BasicConsole.WriteLine(" > > Adding thread object...");
            Threads.Add(mainThread);

            BasicConsole.WriteLine(" > > Setting up memory layout...");
            TheMemoryLayout.CR3 = GetCR3();
        }

        [Compiler.PluggedMethod(ASMFilePath=@"ASM\Processes\Process")]
        public static uint GetCR3()
        {
            return 0;
        }
    }
}
