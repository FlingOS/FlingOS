#define PROCESS_TRACE
#undef PROCESS_TRACE

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

        public Scheduler.Priority Priority;

        private uint ThreadIdGenerator = 0;
        
        public readonly bool UserMode;

        public Process(ThreadStartMethod MainMethod, uint AnId, FOS_System.String AName, bool userMode)
        {
#if PROCESS_TRACE
            Console.Default.WriteLine(" > > Constructing process object...");
#endif
            Id = AnId;
            Name = AName;
            UserMode = userMode;

            CreateThread(MainMethod);
            
#if PROCESS_TRACE
            Console.Default.WriteLine(" > > Setting up memory layout...");
#endif
            TheMemoryLayout.CR3 = GetCR3();
        }

        public void CreateThread(ThreadStartMethod MainMethod)
        {
#if PROCESS_TRACE
            Console.Default.WriteLine(" > > Creating thread...");
#endif
            Thread mainThread = new Thread(MainMethod, ThreadIdGenerator++, UserMode);
#if PROCESS_TRACE
            Console.Default.WriteLine(" > > Adding thread object...");
#endif
            Threads.Add(mainThread);
        }

        [Compiler.PluggedMethod(ASMFilePath=@"ASM\Processes\Process")]
        public static uint GetCR3()
        {
            return 0;
        }
    }
}
