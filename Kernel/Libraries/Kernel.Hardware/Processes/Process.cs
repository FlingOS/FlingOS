#define PROCESS_TRACE
#undef PROCESS_TRACE

using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware;
using Kernel.Hardware.VirtMem;

namespace Kernel.Hardware.Processes
{
    [Compiler.PluggedClass]
    public unsafe class Process : FOS_System.Object
    {
        public List Threads = new List();
        public MemoryLayout TheMemoryLayout = new MemoryLayout();

        public uint Id;
        public FOS_System.String Name;

        public Scheduler.Priority Priority;

        private uint ThreadIdGenerator = 1;
        
        public readonly bool UserMode;

        public Process(ThreadStartMethod MainMethod, uint AnId, FOS_System.String AName, bool userMode)
        {
//#if PROCESS_TRACE
            BasicConsole.WriteLine("Constructing process object...");
//#endif
            Id = AnId;
            Name = AName;
            UserMode = userMode;

//#if PROCESS_TRACE
            BasicConsole.WriteLine("Creating thread...");
//#endif
            CreateThread(MainMethod);
        }

        public void CreateThread(ThreadStartMethod MainMethod)
        {
//#if PROCESS_TRACE
            BasicConsole.WriteLine("Creating thread...");
//#endif
            bool reenable = Scheduler.Enabled;
            if (reenable)
            {
                Scheduler.Disable();
            }

            Thread mainThread = new Thread(MainMethod, ThreadIdGenerator++, UserMode);
#if PROCESS_TRACE
            BasicConsole.WriteLine("Adding data page...");
#endif
            // Add the page to the processes memory layout
            TheMemoryLayout.AddDataPage(
                (uint)VirtMemManager.GetPhysicalAddress(mainThread.State->ThreadStackTop - 4092),
                (uint)mainThread.State->ThreadStackTop - 4092);
            
#if PROCESS_TRACE
            BasicConsole.WriteLine("Adding thread...");
#endif

            Threads.Add(mainThread);

            if (reenable)
            {
                Scheduler.Enable();
            }
        }

        public void SwitchIn()
        {
            TheMemoryLayout.Load(UserMode);
        }
        public void SwitchOut()
        {
            TheMemoryLayout.Unload();
        }
    }
}
