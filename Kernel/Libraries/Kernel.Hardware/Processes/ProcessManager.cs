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

//#define PROCESSMANAGER_TRACE
//#define PROCESSMANAGER_SWITCH_TRACE
//#define PROCESSMANAGER_KERNEL_ACCESS_TRACE

using System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.Processes.Synchronisation;
using Kernel.Hardware.Processes.Synchronisation;
using Kernel.FOS_System.Processes;

namespace Kernel.Hardware.Processes
{
    public static unsafe class ProcessManager
    {
        public const int THREAD_DONT_CARE = -1;

        [Drivers.Compiler.Attributes.Group(Name = "IsolatedKernel_Hardware")]
        public static List Processes = new List();
        [Drivers.Compiler.Attributes.Group(Name = "IsolatedKernel_Hardware")]
        public static Process CurrentProcess = null;
        [Drivers.Compiler.Attributes.Group(Name = "IsolatedKernel_Hardware")]
        public static Thread CurrentThread = null;
        [Drivers.Compiler.Attributes.Group(Name = "IsolatedKernel_Hardware")]
        public static ThreadState* CurrentThread_State = null;

        [Drivers.Compiler.Attributes.Group(Name = "IsolatedKernel_Hardware")]
        public static Process KernelProcess = null;

        [Drivers.Compiler.Attributes.Group(Name = "IsolatedKernel_Hardware")]
        public static uint ProcessIdGenerator = 1;

        [Drivers.Compiler.Attributes.Group(Name = "IsolatedKernel_Hardware")]
        private static List Semaphores = new List(1024, 1024);
        
        public static Process CreateProcess(ThreadStartMethod MainMethod, FOS_System.String Name, bool UserMode)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Creating process object...");
#endif
            Scheduler.Disable();

            Process NewProcess = new Process(MainMethod, ProcessIdGenerator++, Name, UserMode);

            uint[] newDataVAddrs = VirtMemManager.GetBuiltInProcessVAddrs();
            for (int i = 0; i < newDataVAddrs.Length; i++)
            {
                uint vAddr = newDataVAddrs[i];
                uint pAddr = VirtMemManager.GetPhysicalAddress(vAddr);
                void* newVAddr = VirtMemManager.MapFreePages(VirtMem.VirtMemImpl.PageFlags.KernelOnly, 1);
                void* newPAddr = VirtMemManager.GetPhysicalAddress(newVAddr);

                BasicConsole.WriteLine("vAddr=" + (FOS_System.String)vAddr + ", pAddr=" + pAddr + ", newVAddr=" + (uint)newVAddr + ", newPAddr=" + (uint)newPAddr);
                NewProcess.TheMemoryLayout.AddDataPage((uint)newPAddr, (uint)vAddr);

                CurrentProcess.TheMemoryLayout.AddDataPage((uint)newPAddr, (uint)newVAddr);
                uint* srcPtr = (uint*)vAddr;
                uint* dstPtr = (uint*)newVAddr;
                for (uint j = 0; j < 1024; j++, srcPtr++, dstPtr++)
                {
                    *dstPtr = *srcPtr;
                }
                CurrentProcess.TheMemoryLayout.RemovePage((uint)newVAddr);
            }

            Scheduler.Enable();

            return NewProcess;
        }
        public static Process CreateProcess(ThreadStartMethod MainMethod, FOS_System.String Name, bool UserMode, Process ProcessToFork)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Creating process object...");
#endif
            Scheduler.Disable();

            Process NewProcess = new Process(MainMethod, ProcessIdGenerator++, Name, UserMode);

            UInt32Dictionary.Iterator iterator = ProcessToFork.TheMemoryLayout.CodePages.GetNewIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                NewProcess.TheMemoryLayout.AddCodePage(pair.Value & 0xFFFFF000, pair.Key);
            }

            BasicConsole.WriteLine("Forking " + ProcessToFork.Name);

            iterator = ProcessToFork.TheMemoryLayout.DataPages.GetNewIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();

                uint vAddr = pair.Key;
                uint pAddr = pair.Value & 0xFFFFF000;
                void* newVAddr = VirtMemManager.MapFreePages(VirtMem.VirtMemImpl.PageFlags.KernelOnly, 1);
                void* newPAddr = VirtMemManager.GetPhysicalAddress(newVAddr);

                BasicConsole.WriteLine("vAddr=" + (FOS_System.String)vAddr + ", pAddr=" + pAddr + ", newVAddr=" + (uint)newVAddr + ", newPAddr=" + (uint)newPAddr);
                NewProcess.TheMemoryLayout.AddDataPage((uint)newPAddr, (uint)vAddr);

                CurrentProcess.TheMemoryLayout.AddDataPage((uint)newPAddr, (uint)newVAddr);
                uint* srcPtr = (uint*)vAddr;
                uint* dstPtr = (uint*)newVAddr;
                for (uint i = 0; i < 1024; i++, srcPtr++, dstPtr++)
                {
                    *dstPtr = *srcPtr;
                }
                CurrentProcess.TheMemoryLayout.RemovePage((uint)newVAddr);
            }

            Scheduler.Enable();

            return NewProcess;
        }
        public static void RegisterProcess(Process process, Scheduler.Priority priority)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Registering process...");
#endif
            if (process == null)
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Attempted to register null process!"));
            }

            if (process.Registered)
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Attempted to re-register process! Process name: " + process.Name));
            }
            
#if PROCESSMANAGER_TRACE
            //BasicConsole.WriteLine("Disabling scheduler...");
#endif
            //bool reenable = Scheduler.Enabled;
            //if (reenable)
            //{
            //    Scheduler.Disable();
            //}

#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Adding process...");
#endif

            Processes.Add(process);

#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Initialising process...");
#endif
            Scheduler.InitProcess(process, priority);


#if PROCESSMANAGER_TRACE
            //BasicConsole.WriteLine("Enabling scheduler...");
#endif
            //if (reenable)
            //{
            //    Scheduler.Enable();
            //}
        }

        public static Process GetProcessById(uint processId)
        {
            for (int i = 0; i < Processes.Count; i++)
            {
                if (((Process)Processes[i]).Id == processId)
                {
                    return ((Process)Processes[i]);
                }
            }
            return null;
        }
        public static Thread GetThreadById(uint threadId, Process parent)
        {
            for (int i = 0; i < parent.Threads.Count; i++)
            {
                if (((Thread)parent.Threads[i]).Id == threadId)
                {
                    return (Thread)parent.Threads[i];
                }
            }
            return null;
        }

        /// <remarks>
        /// Specifying threadId=-1 accepts any thread from the specified process.
        /// No guarantees are made about the thread chosen. This is used when you
        /// mainly want to switch process context and don't care about the specific
        /// thread context e.g. during an interrupt.
        /// </remarks>
        [Drivers.Compiler.Attributes.NoDebug]
        public static void SwitchProcess(uint processId, int threadId)
        {
            //Switch the current memory layout across.
            //  Don't touch register state etc, just the memory layout

            bool dontSwitchOutIn = false;

            if (CurrentProcess != null &&
                CurrentProcess.Id == processId)
            {
                if (CurrentThread != null &&
                    (CurrentThread.Id == threadId || threadId == THREAD_DONT_CARE))
                {
#if PROCESSMANAGER_SWITCH_TRACE
                    BasicConsole.WriteLine("No switch. (1)");
#endif
                    return;
                }
                else
                {
#if PROCESSMANAGER_SWITCH_TRACE
                    BasicConsole.WriteLine("No switch. (2)");
#endif
                    dontSwitchOutIn = true;
                }
            }

            if (!dontSwitchOutIn)
            {
#if PROCESSMANAGER_SWITCH_TRACE
                BasicConsole.Write("Switching out: ");
                BasicConsole.WriteLine(CurrentProcess.Name);
#endif
                
                Process NewProcess = GetProcessById(processId);

                // Process not found
                if (NewProcess == null)
                {
#if PROCESSMANAGER_SWITCH_TRACE
                    BasicConsole.WriteLine("Process not found.");
#endif
                    return;
                }

#if PROCESSMANAGER_SWITCH_TRACE
                BasicConsole.Write("Switching in: ");
                BasicConsole.WriteLine(NewProcess.Name);
#endif
                NewProcess.SwitchFromLayout(CurrentProcess.TheMemoryLayout);
                CurrentProcess = NewProcess;
#if PROCESSMANAGER_SWITCH_TRACE
                BasicConsole.Write("Switched in.");
#endif
            }

            CurrentThread = null;
            CurrentThread_State = null;

            if (threadId == THREAD_DONT_CARE)
            {
                if (CurrentProcess.Threads.Count > 0)
                {
                    CurrentThread = (Thread)CurrentProcess.Threads[0];
                }
            }
            else
            {
                CurrentThread = GetThreadById((uint)threadId, CurrentProcess);
            }

            // No threads in the process (?!) or process not found
            if (CurrentThread == null)
            {
#if PROCESSMANAGER_SWITCH_TRACE
                BasicConsole.WriteLine("Thread not found.");
#endif
                return;
            }
#if PROCESSMANAGER_SWITCH_TRACE
            BasicConsole.WriteLine("Thread found.");
#endif

            CurrentThread_State = CurrentThread.State;
#if PROCESSMANAGER_SWITCH_TRACE
            BasicConsole.WriteLine("Thread state updated.");
#endif
        }

        public static bool WakeProcess(uint processId, uint threadId)
        {
            return WakeThread(GetProcessById(processId), threadId);
        }
        public static bool WakeThread(Process theProcess, uint threadId)
        {
            bool Woken = false;

            if (theProcess != null)
            {
                Thread theThread = GetThreadById(threadId, theProcess);
                if (theThread != null)
                {
                    theThread._Wake();
                    Woken = true;
                }
            }

            return Woken;
        }

        public static int Semaphore_Allocate(int limit, Process aProcess)
        {
            int result = -1;
            Semaphore theSemaphore = null;

            for (int i = 0; i < Semaphores.Count; i++)
            {
                Semaphore aSemaphore = (Semaphore)Semaphores[i];
                if (aSemaphore == null)
                {
                    Semaphores[i] = theSemaphore = new Semaphore(i, limit);
                    result = i;
                    break;
                }
            }

            if (result == -1)
            {
                result = Semaphores.Count;
                Semaphores.Add(theSemaphore = new Semaphore(result, limit));
            }

            theSemaphore.OwnerProcesses.Add(aProcess.Id);

            return result;
        }
        public static bool Semaphore_Deallocate(int id)
        {
            if (Semaphore_CheckExists(id))
            {
                Semaphores[id] = null;
                
                return true;
            }
            return false;
        }
        public static int Semaphore_Wait(int id, Process aProcess, Thread aThread)
        {
            if (Semaphore_VerifyOwner(id, aProcess))
            {
                return ((Semaphore)Semaphores[id]).WaitOnBehalf(aProcess, aThread) ? 1 : 0;
            }
            return -1;
        }
        public static bool Semaphore_Signal(int id, Process aProcess)
        {
            if (Semaphore_VerifyOwner(id, aProcess))
            {
                ((Semaphore)Semaphores[id]).SignalOnBehalf();
                return true;
            }
            return false;
        }
        public static bool Semaphore_AddOwner(int semaphoreId, uint NewProcessId, Process CallerProcess)
        {
            if (Semaphore_VerifyOwner(semaphoreId, CallerProcess))
            {
                ((Semaphore)Semaphores[semaphoreId]).OwnerProcesses.Add(NewProcessId);
                return true;
            }
            return false;
        }
        public static bool Semaphore_RemoveOwner(int semaphoreId, uint RemoveProcessId, Process CallerProcess)
        {
            if (Semaphore_VerifyOwner(semaphoreId, CallerProcess) && RemoveProcessId == CallerProcess.Id)
            {
                ((Semaphore)Semaphores[semaphoreId]).OwnerProcesses.Remove(RemoveProcessId);
                return true;
            }
            return false;
        }
        public static bool Semaphore_CheckForDeallocate(int semaphoreId)
        {
            if (Semaphore_CheckExists(semaphoreId))
            {
                if (((Semaphore)Semaphores[semaphoreId]).OwnerProcesses.Count == 0)
                {
                    return Semaphore_Deallocate(semaphoreId);
                }
            }
            return false;
        }
        private static bool Semaphore_VerifyOwner(int id, Process aProcess)
        {
            if (Semaphore_CheckExists(id))
            {
                Semaphore theSemaphore = ((Semaphore)Semaphores[id]);
                return theSemaphore.OwnerProcesses.IndexOf(aProcess.Id) > -1;
            }
            return false;
        }
        private static bool Semaphore_CheckExists(int id)
        {
            return id > -1 && id < Semaphores.Count && Semaphores[id] != null;
        }

        public static void EnableKernelAccessToProcessMemory(uint TargetProcessId)
        {
            EnableKernelAccessToProcessMemory(GetProcessById(TargetProcessId));
        }
        public static void DisableKernelAccessToProcessMemory(uint TargetProcessId)
        {
            DisableKernelAccessToProcessMemory(GetProcessById(TargetProcessId));
        }
        public static void EnableKernelAccessToProcessMemory(Process TargetProcess)
        {
            if (KernelProcess != null && KernelProcess != TargetProcess)
            {
#if PROCESSMANAGER_KERNEL_ACCESS_TRACE
                BasicConsole.WriteLine("~E~");
#endif

                KernelProcess.TheMemoryLayout.Merge(TargetProcess.TheMemoryLayout, false);

#if PROCESSMANAGER_KERNEL_ACCESS_TRACE
                BasicConsole.WriteLine("¬E¬");
#endif
            }
        }
        public static void DisableKernelAccessToProcessMemory(Process TargetProcess)
        {
            if (KernelProcess != null && KernelProcess != TargetProcess)
            {
#if PROCESSMANAGER_KERNEL_ACCESS_TRACE
                BasicConsole.WriteLine("~D~");
#endif

                KernelProcess.TheMemoryLayout.Unmerge(TargetProcess.TheMemoryLayout);

#if PROCESSMANAGER_KERNEL_ACCESS_TRACE
                BasicConsole.WriteLine("¬D¬");
#endif
            }
        }
    }
}
