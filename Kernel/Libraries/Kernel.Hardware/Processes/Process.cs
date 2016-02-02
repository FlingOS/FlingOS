#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //
#endregion
    
//#define PROCESS_TRACE

using System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.Processes.Synchronisation;
using Kernel.Hardware;
using Kernel.Hardware.VirtMem;
using Kernel.FOS_System.Processes;

namespace Kernel.Hardware.Processes
{
    public unsafe class Process : FOS_System.Object
    {
        public List Threads;
        public MemoryLayout TheMemoryLayout;

        public uint Id;
        public FOS_System.String Name;
        public Scheduler.Priority Priority;
        public readonly bool UserMode;

        protected uint ThreadIdGenerator = 1;

        public bool SwitchProcessForISRs = true;
        public Bitmap ISRsToHandle = new Bitmap(256);
        public ISRHanderDelegate ISRHandler = null;
        public bool SwitchProcessForIRQs = true;
        public Bitmap IRQsToHandle = new Bitmap(256);
        public IRQHanderDelegate IRQHandler = null;
        public Bitmap SyscallsToHandle = new Bitmap(256);
        public SyscallHanderDelegate SyscallHandler = null;

        public FOS_System.HeapBlock* HeapPtr = null;
        public SpinLock HeapLock = null;
        public FOS_System.GCState TheGCState = null;
        public bool OutputMemTrace = false;

        public bool Registered = false;

        public Process(ThreadStartMethod MainMethod, uint AnId, FOS_System.String AName, bool userMode)
        {
#if PROCESS_TRACE
            BasicConsole.WriteLine("Constructing process object...");
#endif
            Id = AnId;
            Name = AName;
            UserMode = userMode;

#if PROCESS_TRACE
            BasicConsole.WriteLine("Process: ctor: Initialising fields...");
#endif

            Threads = new List();
            TheMemoryLayout = new MemoryLayout();

#if PROCESS_TRACE
            BasicConsole.WriteLine("Process: ctor: Initialising memory layout...");
#endif
            
#if PROCESS_TRACE
            BasicConsole.WriteLine("Process: ctor: Creating thread...");
#endif
            CreateThread(MainMethod, "Main");
        }

        public virtual Thread CreateThread(ThreadStartMethod MainMethod, FOS_System.String Name)
        {
#if PROCESS_TRACE
            BasicConsole.WriteLine("Process: CreateThread: Creating thread...");
#endif

            try
            {
                // Required so that page allocations by new Thread don't create conflicts
                ProcessManager.EnableKernelAccessToProcessMemory(this);

                Thread newThread = new Thread(this, MainMethod, ThreadIdGenerator++, UserMode, Name);
#if PROCESS_TRACE
            BasicConsole.WriteLine("Adding data page...");
#endif
                // Add the page to the processes memory layout
                uint threadStackVirtAddr = (uint)newThread.State->ThreadStackTop - 4092;
                uint threadStackPhysAddr = (uint)VirtMemManager.GetPhysicalAddress(newThread.State->ThreadStackTop - 4092);
                TheMemoryLayout.AddDataPage(threadStackPhysAddr, threadStackVirtAddr);
                
#if PROCESS_TRACE
            BasicConsole.WriteLine("Adding thread...");
#endif

                Threads.Add(newThread);

                if (Registered)
                {
                    Scheduler.InitThread(this, newThread);
                }

                return newThread;
            }
            finally
            {
                ProcessManager.DisableKernelAccessToProcessMemory(this);
            }
        }

        public virtual void LoadMemLayout()
        {
            TheMemoryLayout.Load(UserMode);
        }
        public virtual void UnloadMemLayout()
        {
            //BasicConsole.WriteLine("Process Unload calling MemoryLayout unload");
            TheMemoryLayout.Unload();
        }
        public virtual void SwitchFromLayout(MemoryLayout old)
        {
            TheMemoryLayout.SwitchFrom(UserMode, old);
        }

        public void SuspendThreads()
        {
            for (int i = 0; i < Threads.Count; i++)
            {
                ((Thread)Threads[i]).Suspend = true;
            }
        }
        public void ResumeThreads()
        {
            for (int i = 0; i < Threads.Count; i++)
            {
                ((Thread)Threads[i]).Suspend = false;
            }
        }
    }
}
