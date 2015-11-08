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
#undef PROCESS_TRACE

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
            CreateThread(MainMethod, "Main");

            if (createHeap)
            {
                CreateHeap();
            }
        }

        public virtual Thread CreateThread(ThreadStartMethod MainMethod, FOS_System.String Name)
        {
#if PROCESS_TRACE
            BasicConsole.WriteLine("Creating thread...");
#endif
            //bool reenable = Scheduler.Enabled;
            //if (reenable)
            //{
            //    Scheduler.Disable();
            //}

            Thread newThread = new Thread(MainMethod, ThreadIdGenerator++, UserMode, Name);
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
            BasicConsole.WriteLine("Allocating memory for heap...");
#endif
            // Allocate memory for new heap
            uint heapPages = 64; // 256KiB, page-aligned
            FOS_System.HeapBlock* heapPtr = (FOS_System.HeapBlock*)VirtMemManager.MapFreePages(
                                UserMode ? VirtMemImpl.PageFlags.None :
                                           VirtMemImpl.PageFlags.KernelOnly, (int)heapPages);
#if PROCESS_TRACE
            BasicConsole.WriteLine("Generating physical addresses...");
#endif
            uint[] pAddrs = new uint[heapPages];
            for (uint currPtr = (uint)heapPtr, i = 0; i < heapPages; currPtr += 4096, i++)
            {
                pAddrs[i] = VirtMemManager.GetPhysicalAddress(currPtr);
            }

#if PROCESS_TRACE
            BasicConsole.WriteLine("Adding memory to current process (kernel task) layout...");
#endif
            // Add heap memory to current (kernel) process's memory
            //  - Makes sure it won't be mapped out during initialisation
            ProcessManager.CurrentProcess.TheMemoryLayout.AddDataPages((uint)heapPtr, pAddrs);
            // Force reload of the memory layout
            ProcessManager.CurrentProcess.TheMemoryLayout.Load(ProcessManager.CurrentProcess.UserMode);

#if PROCESS_TRACE
            BasicConsole.WriteLine("Initialising heap...");
#endif
            // Initialise the heap
            FOS_System.Heap.InitBlock(heapPtr, heapPages * 4096, 32);

#if PROCESS_TRACE
            BasicConsole.WriteLine("Adding memory to layout...");
#endif
            // Add allocated new process's memory to layout
            TheMemoryLayout.AddDataPages((uint)heapPtr, pAddrs);

#if PROCESS_TRACE
            BasicConsole.WriteLine("Removing memory from current process (kernel task) layout...");
#endif
            // Remove heap memory from current (kernel) process's memory
            ProcessManager.CurrentProcess.TheMemoryLayout.RemovePages((uint)heapPtr, heapPages);

#if PROCESS_TRACE
            BasicConsole.WriteLine("Setting heap pointer...");
#endif
            // Set heap pointer
            HeapPtr = heapPtr;
        }
        public void InitHeap()
        {
#if PROCESS_TRACE
            BasicConsole.WriteLine(" > Initialising process heap...");
            if (FOS_System.GC.State != null)
            {
                BasicConsole.WriteLine(" > !! GC State not null!");
            }
            else
            {
                BasicConsole.WriteLine(" > GC State null as expected.");
            }

            BasicConsole.WriteLine(" >> Creating heap lock...");
#endif
            HeapLock = new SpinLock();
#if PROCESS_TRACE
            BasicConsole.WriteLine(" >> Setting heap lock...");
#endif
            FOS_System.Heap.AccessLock = HeapLock;
#if PROCESS_TRACE
            BasicConsole.WriteLine(" >> Setting heap lock initialised...");
#endif
            FOS_System.Heap.AccessLockInitialised = true;
            
#if PROCESS_TRACE
            BasicConsole.WriteLine(" >> Creating new GC state...");
#endif
            TheGCState = new FOS_System.GCState();

            if ((uint)TheGCState.CleanupList == 0xFFFFFFFF)
            {
                BasicConsole.WriteLine(" !!! PANIC !!! ");
                BasicConsole.WriteLine(" GC.state.CleanupList is 0xFFFFFFFF NOT null!");
                BasicConsole.WriteLine(" !-!-!-!-!-!-! ");
            }
#if PROCESS_TRACE
            BasicConsole.WriteLine(" >> Creating new GC lock...");
#endif
            TheGCState.AccessLock = new SpinLock();
#if PROCESS_TRACE
            BasicConsole.WriteLine(" >> Setting GC lock initialised...");
#endif
            TheGCState.AccessLockInitialised = true;
#if PROCESS_TRACE
            BasicConsole.WriteLine(" >> Setting GC state...");
#endif
            FOS_System.GC.State = TheGCState;
#if PROCESS_TRACE
            BasicConsole.WriteLine(" >> Done.");
#endif
        }
        public void UnloadHeap()
        {
            FOS_System.Heap.OutputTrace = OutputMemTrace;
            FOS_System.GC.OutputTrace = OutputMemTrace;
        }
        public void LoadHeap()
        {
            if (HeapPtr != null)
            {
                FOS_System.Heap.Load(HeapPtr, HeapLock);
                FOS_System.GC.Load(TheGCState);

                FOS_System.GC.OutputTrace = OutputMemTrace;
            }

            FOS_System.Heap.name = ProcessManager.CurrentProcess.Name;
            FOS_System.Heap.OutputTrace = OutputMemTrace;
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
