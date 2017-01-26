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

using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Processes;
using Kernel.VirtualMemory;

namespace Kernel.Multiprocessing
{
    public sealed unsafe class Process : Object
    {
        public readonly bool UserMode;

        public uint Id;
        public IRQHanderDelegate IRQHandler = null;
        public Bitmap IRQsToHandle = new Bitmap(256);
        public ISRHanderDelegate ISRHandler = null;
        public Bitmap ISRsToHandle = new Bitmap(256);
        public String Name;
        public Scheduler.Priority Priority;

        public bool Registered = false;
        public bool SwitchProcessForIRQs = true;

        public bool SwitchProcessForISRs = true;
        public SyscallHanderDelegate SyscallHandler = null;
        public Bitmap SyscallsToHandle = new Bitmap(256);
        public MemoryLayout TheMemoryLayout;

        protected uint ThreadIdGenerator = 1;
        public List Threads;

        public Process(ThreadStartPoint StartPoint, uint AnId, String AName, bool userMode, uint[] StartArgs)
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
            CreateThread(StartPoint, "Main", StartArgs);
        }

        public Thread CreateThread(ThreadStartPoint StartPoint, String Name, uint[] StartArgs)
        {
#if PROCESS_TRACE
            BasicConsole.WriteLine("Process: CreateThread: Creating thread...");
#endif

            try
            {
                // Required so that page allocations by new Thread don't create conflicts
                ProcessManager.EnableKernelAccessToProcessMemory(this);

#if PAGING_TRACE
                BasicConsole.Write("Physical address of 0x00106000 is ");
                Framework.String valStr = "0x        ";
                ExceptionMethods.FillString(VirtMemManager.GetPhysicalAddress(0x00106000), 9, valStr);
                BasicConsole.WriteLine(valStr);
#endif
                void* threadStackPhysAddr;
                void* kernelStackPhysAddr;
                Thread newThread = new Thread(this, StartPoint, ThreadIdGenerator++, UserMode, Name,
                    out threadStackPhysAddr, out kernelStackPhysAddr, StartArgs);
#if PROCESS_TRACE
            BasicConsole.WriteLine("Adding data page...");
#endif
                // Add the page to the processes memory layout
                uint threadStackVirtAddr = (uint)newThread.State->ThreadStackTop - Thread.ThreadStackTopOffset;
                uint kernelStackVirtAddr = (uint)newThread.State->KernelStackTop - Thread.KernelStackTopOffset;
                if (ProcessManager.KernelProcess != null && this != ProcessManager.KernelProcess)
                {
                    TheMemoryLayout.AddDataPage((uint)threadStackPhysAddr, threadStackVirtAddr);
                    TheMemoryLayout.AddKernelPage((uint)kernelStackPhysAddr, kernelStackVirtAddr);
                    ProcessManager.KernelProcess.TheMemoryLayout.AddKernelPage((uint)kernelStackPhysAddr,
                        kernelStackVirtAddr);
                }
                else
                {
                    TheMemoryLayout.AddKernelPage((uint)kernelStackPhysAddr, kernelStackVirtAddr);
                }

#if PROCESS_TRACE
                if (Name != "Main")
                {
                    BasicConsole.WriteLine("New thread info:");
                    BasicConsole.WriteLine("Name : " + Name);
                    BasicConsole.WriteLine("Thread stack : " + (Framework.String)threadStackVirtAddr + " => " + (uint)threadStackPhysAddr);
                    BasicConsole.WriteLine("Kernel stack : " + (Framework.String)kernelStackVirtAddr + " => " + (uint)kernelStackPhysAddr);
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

        public void SwitchFromLayout(MemoryLayout old)
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