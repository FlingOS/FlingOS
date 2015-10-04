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
    
#define PROCESS_TRACE
//#undef PROCESS_TRACE

using System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.Processes.Synchronisation;
using Kernel.Hardware;
using Kernel.Hardware.VirtMem;

namespace Kernel.Hardware.Processes
{
    public delegate int ISRHanderDelegate(uint isrNumber);
    public delegate int IRQHanderDelegate(uint irqNumber);
    public delegate int SyscallHanderDelegate(uint syscallNumber, uint param1, uint param2, uint param3, 
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcesId, uint callerThreadId);

    public unsafe class Process : FOS_System.Object
    {
        public List Threads = new List();
        public MemoryLayout TheMemoryLayout = new MemoryLayout();

        public uint Id;
        public FOS_System.String Name;
        public Scheduler.Priority Priority;
        public readonly bool UserMode;

        protected uint ThreadIdGenerator = 1;
                
        public Bitmap ISRsToHandle = new Bitmap(256);
        public ISRHanderDelegate ISRHandler = null;
        public Bitmap IRQsToHandle = new Bitmap(256);
        public IRQHanderDelegate IRQHandler = null;
        public Bitmap SyscallsToHandle = new Bitmap(256);
        public SyscallHanderDelegate SyscallHandler = null;

        public FOS_System.HeapBlock* HeapPtr = null;
        public SpinLock HeapLock = null;
        public FOS_System.ObjectToCleanup* GCCleanupListPtr = null;
        public SpinLock GCLock = null;
        public bool InsideGC = false;
        public bool OutputMemTrace = false;

        public Process(ThreadStartMethod MainMethod, uint AnId, FOS_System.String AName, bool userMode, bool createHeap)
        {
#if PROCESS_TRACE
            BasicConsole.WriteLine("Constructing process object...");
#endif
            Id = AnId;
            Name = AName;
            UserMode = userMode;

#if PROCESS_TRACE
            BasicConsole.WriteLine("Creating thread...");
#endif
            CreateThread(MainMethod);

            if (createHeap)
            {
                CreateHeap();
            }
        }

        public virtual Thread CreateThread(ThreadStartMethod MainMethod)
        {
#if PROCESS_TRACE
            BasicConsole.WriteLine("Creating thread...");
#endif
            //bool reenable = Scheduler.Enabled;
            //if (reenable)
            //{
            //    Scheduler.Disable();
            //}

            Thread newThread = new Thread(MainMethod, ThreadIdGenerator++, UserMode);
#if PROCESS_TRACE
            BasicConsole.WriteLine("Adding data page...");
#endif
            // Add the page to the processes memory layout
            uint threadStackVirtAddr = (uint)newThread.State->ThreadStackTop - 4092;
            uint threadStackPhysAddr = (uint)VirtMemManager.GetPhysicalAddress(newThread.State->ThreadStackTop - 4092);
            TheMemoryLayout.AddDataPage(threadStackPhysAddr, threadStackVirtAddr);
            if (ProcessManager.KernelProcess != null)
            {
                ProcessManager.KernelProcess.TheMemoryLayout.AddDataPage(threadStackPhysAddr, threadStackVirtAddr);
            }

#if PROCESS_TRACE
            BasicConsole.WriteLine("Adding thread...");
#endif

            Threads.Add(newThread);

            //if (reenable)
            //{
            //    Scheduler.Enable();
            //}

            return newThread;
        }

        private void CreateHeap()
        {
#if PROCESS_TRACE
            BasicConsole.WriteLine("Creating new heap lock...");
#endif

            // Create new heap lock
            HeapLock = new SpinLock(-1);
            
#if PROCESS_TRACE
            BasicConsole.WriteLine("Creating new GC lock...");
#endif

            // Create new heap lock
            GCLock = new SpinLock(-1);
            
#if PROCESS_TRACE
            BasicConsole.WriteLine("Allocating memory for heap...");
#endif
            // Allocate memory for new heap
            FOS_System.HeapBlock* heapPtr = (FOS_System.HeapBlock*)VirtMemManager.MapFreePages(
                                UserMode ? VirtMemImpl.PageFlags.None :
                                           VirtMemImpl.PageFlags.KernelOnly, 256); // 1 MiB, page-aligned
#if PROCESS_TRACE
            BasicConsole.WriteLine("Generating physical addresses...");
#endif
            uint[] pAddrs = new uint[256];
            for (uint currPtr = (uint)heapPtr, i = 0; i < 256; currPtr += 4096, i++)
            {
                pAddrs[i] = VirtMemManager.GetPhysicalAddress(currPtr);
            }

#if PROCESS_TRACE
            BasicConsole.WriteLine("Adding memory to current process (kernel task) layout...");
#endif
            // Add heap memory to current (kernel) process's memory
            //  - Makes sure it won't be mapped out during initialisation
            ProcessManager.CurrentProcess.TheMemoryLayout.AddDataPages((uint)heapPtr, pAddrs);
            // Brief pause to make the scheduler (unload/)load the data layout
            Thread.Sleep(1000);

#if PROCESS_TRACE
            BasicConsole.WriteLine("Initialising heap...");
#endif
            // Initialise the heap
            FOS_System.Heap.InitBlock(heapPtr, 256 * 4096, 32);

#if PROCESS_TRACE
            BasicConsole.WriteLine("Adding memory to layout...");
#endif
            // Add allocated new process's memory to layout
            TheMemoryLayout.AddDataPages((uint)heapPtr, pAddrs);

#if PROCESS_TRACE
            BasicConsole.WriteLine("Removing memory from current process (kernel task) layout...");
#endif
            // Remove heap memory from current (kernel) process's memory
            ProcessManager.CurrentProcess.TheMemoryLayout.RemovePages((uint)heapPtr, 256);
            // Brief pause to make the scheduler (unload/)load the data layout
            Thread.Sleep(1000);

#if PROCESS_TRACE
            BasicConsole.WriteLine("Setting heap pointer...");
#endif
            // Set heap pointer
            HeapPtr = heapPtr;
        }
        public void UnloadHeap()
        {
            InsideGC = FOS_System.GC.InsideGC;

            FOS_System.Heap.OutputTrace = OutputMemTrace;
            FOS_System.GC.OutputTrace = OutputMemTrace;
        }
        public void LoadHeap()
        {
            FOS_System.Heap.name = ProcessManager.CurrentProcess.Name;
            FOS_System.Heap.OutputTrace = OutputMemTrace;
            FOS_System.GC.OutputTrace = OutputMemTrace;

            if (HeapPtr != null)
            {
                FOS_System.GC.InsideGC = InsideGC;
                FOS_System.Heap.LoadHeap(HeapPtr, HeapLock);
                FOS_System.GC.LoadGC(GCCleanupListPtr, GCLock);
            }
        }

        public virtual void LoadMemLayout()
        {
            TheMemoryLayout.Load(UserMode);
        }
        public virtual void UnloadMemLayout()
        {
            TheMemoryLayout.Unload();
        }
    }
}
