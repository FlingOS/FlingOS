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
        
        public bool Registered = false;

        public Process(ThreadStartPoint StartPoint, uint AnId, FOS_System.String AName, bool userMode)
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
            TheMemoryLayout.AddAllDataToKernel = ProcessManager.KernelProcess == null;

#if PROCESS_TRACE
            BasicConsole.WriteLine("Process: ctor: Initialising memory layout...");
#endif
            
#if PROCESS_TRACE
            BasicConsole.WriteLine("Process: ctor: Creating thread...");
#endif
            CreateThread(StartPoint, "Main");
        }

        public virtual Thread CreateThread(ThreadStartPoint StartPoint, FOS_System.String Name)
        {
#if PROCESS_TRACE
            BasicConsole.WriteLine("Process: CreateThread: Creating thread...");
#endif

            try
            {
                // Required so that page allocations by new Thread don't create conflicts
                ProcessManager.EnableKernelAccessToProcessMemory(this);

                bool reenable = Scheduler.Enabled;
                if (reenable)
                {
                    //TODO: Not ideal
                    Scheduler.Disable();
                }

#if PAGING_TRACE
                BasicConsole.Write("Physical address of 0x00106000 is ");
                FOS_System.String valStr = "0x        ";
                ExceptionMethods.FillString(VirtMemManager.GetPhysicalAddress(0x00106000), 9, valStr);
                BasicConsole.WriteLine(valStr);
#endif

                Thread newThread = new Thread(this, StartPoint, ThreadIdGenerator++, UserMode, Name);
#if PROCESS_TRACE
            BasicConsole.WriteLine("Adding data page...");
#endif
                // Add the page to the processes memory layout
                uint threadStackVirtAddr = (uint)newThread.State->ThreadStackTop - Thread.ThreadStackTopOffset;
                uint threadStackPhysAddr = (uint)VirtMemManager.GetPhysicalAddress(newThread.State->ThreadStackTop - Thread.ThreadStackTopOffset);
                uint kernelStackVirtAddr = (uint)newThread.State->KernelStackTop - Thread.KernelStackTopOffset;
                uint kernelStackPhysAddr = (uint)VirtMemManager.GetPhysicalAddress(newThread.State->KernelStackTop - Thread.KernelStackTopOffset);

                if (reenable)
                {
                    //TODO: Not ideal
                    Scheduler.Enable();
                }

                TheMemoryLayout.AddDataPage(threadStackPhysAddr, threadStackVirtAddr);
                TheMemoryLayout.AddKernelPage(kernelStackPhysAddr, kernelStackVirtAddr);
                
                if (ProcessManager.KernelProcess != null && this != ProcessManager.KernelProcess)
                {
                    ProcessManager.KernelProcess.TheMemoryLayout.AddKernelPage(kernelStackPhysAddr, kernelStackVirtAddr);
                }

#if PROCESS_TRACE
                if (Name != "Main")
                {
                    BasicConsole.WriteLine("New thread info:");
                    BasicConsole.WriteLine("Name : " + Name);
                    BasicConsole.WriteLine("Thread stack : " + (FOS_System.String)threadStackVirtAddr + " => " + threadStackPhysAddr);
                    BasicConsole.WriteLine("Kernel stack : " + (FOS_System.String)kernelStackVirtAddr + " => " + kernelStackPhysAddr);
                }

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
