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

using Drivers.Compiler.Attributes;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Processes;
using Kernel.Multiprocessing.Synchronisation;
using Kernel.Utilities;
using Kernel.VirtualMemory;

namespace Kernel.Multiprocessing
{
    public static unsafe class ProcessManager
    {
        public const int THREAD_DONT_CARE = -1;

        [Group(Name = "IsolatedKernel_Hardware_Multiprocessing")] public static List Processes = new List();

        [Group(Name = "IsolatedKernel_Hardware_Multiprocessing")] public static Process CurrentProcess;

        [Group(Name = "IsolatedKernel_Hardware_Multiprocessing")] public static Thread CurrentThread;

        [Group(Name = "IsolatedKernel_Hardware_Multiprocessing")] public static ThreadState* CurrentThread_State = null;

        [Group(Name = "IsolatedKernel_Hardware_Multiprocessing")] public static Process KernelProcess = null;

        [Group(Name = "IsolatedKernel_Hardware_Multiprocessing")] public static uint ProcessIdGenerator = 1;

        [Group(Name = "IsolatedKernel_Hardware_Multiprocessing")] private static readonly List Semaphores =
            new List(1024, 1024);


        private static void* ShadowPageVAddr = (void*)0xFFFFFFFF;
        private static void* ShadowPagePAddr = (void*)0xFFFFFFFF;

        public static Process CreateProcess(ThreadStartPoint MainMethod, String Name, bool UserMode, uint[] StartArgs)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Creating process...");
#endif

            Process NewProcess = new Process(MainMethod, ProcessIdGenerator++, Name, UserMode, StartArgs);

            uint[] vAddrs = VirtualMemoryManager.GetBuiltInProcessVAddrs();
            uint startVAddr = 0xDEADBEEF;
            uint vAddrCount = 0;
            for (int i = 0; i < vAddrs.Length; i++)
            {
                if (vAddrCount > 0)
                {
                    if (startVAddr + 0x1000*vAddrCount != vAddrs[i])
                    {
                        AddDataPagesToProcess(NewProcess, startVAddr, vAddrCount);

                        vAddrCount = 0;
                    }
                }

                if (vAddrCount == 0)
                {
                    startVAddr = vAddrs[i];
                }
                vAddrCount++;
            }
            if (vAddrCount > 0)
            {
                AddDataPagesToProcess(NewProcess, startVAddr, vAddrCount);
            }

#if PROCESSMANAGER_TRACE
            {
                BasicConsole.WriteLine("New process memory layout:");
                BasicConsole.WriteLine(" - Code pages:");
                UInt32Dictionary.Iterator iterator = NewProcess.TheMemoryLayout.CodePages.GetNewIterator();
                while (iterator.HasNext())
                {
                    UInt32Dictionary.KeyValuePair pair = iterator.Next();
                    BasicConsole.WriteLine("VAddr: " + (Framework.String)pair.Key + " => " + pair.Value);
                }
                BasicConsole.WriteLine(" - Data pages:");
                iterator = NewProcess.TheMemoryLayout.DataPages.GetNewIterator();
                while (iterator.HasNext())
                {
                    UInt32Dictionary.KeyValuePair pair = iterator.Next();
                    BasicConsole.WriteLine("VAddr: " + (Framework.String)pair.Key + " => " + pair.Value);
                }
                BasicConsole.WriteLine(" - Kernel pages:");
                iterator = NewProcess.TheMemoryLayout.KernelPages.GetNewIterator();
                while (iterator.HasNext())
                {
                    UInt32Dictionary.KeyValuePair pair = iterator.Next();
                    BasicConsole.WriteLine("VAddr: " + (Framework.String)pair.Key + " => " + pair.Value);
                }
            }
#endif

            return NewProcess;
        }

        public static Process CreateProcess(bool UserMode, Process ProcessToCopyFrom, StartProcessRequest* request, uint[] StartArgs)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Creating process...");
#endif

            EnableKernelAccessToProcessMemory(ProcessToCopyFrom);

            String Name = String.New(request->NameLength);
            for (int i = 0; i < request->NameLength; i++)
            {
                Name[i] = request->Name[i];
            }
            Process NewProcess = new Process((ThreadStartPoint)ObjectUtilities.GetObject(request->MainMethod),
                ProcessIdGenerator++, Name, UserMode, StartArgs);

            uint* vAddrs = request->CodePages;
            uint startVAddr = 0xDEADBEEF;
            uint vAddrCount = 0;
            for (int i = 0; i < request->CodePagesCount; i++)
            {
                if (vAddrCount > 0)
                {
                    if (startVAddr + 0x1000*vAddrCount != vAddrs[i])
                    {
                        AddCodePagesToProcess(NewProcess, startVAddr, vAddrCount);

                        vAddrCount = 0;
                    }
                }

                if (vAddrCount == 0)
                {
                    startVAddr = vAddrs[i];
                }
                vAddrCount++;
            }
            if (vAddrCount > 0)
            {
                AddCodePagesToProcess(NewProcess, startVAddr, vAddrCount);
            }

            vAddrs = request->DataPages;
            startVAddr = 0xDEADBEEF;
            vAddrCount = 0;
            for (int i = 0; i < request->DataPagesCount; i++)
            {
                if (vAddrCount > 0)
                {
                    if (startVAddr + 0x1000*vAddrCount != vAddrs[i])
                    {
                        AddDataPagesToProcess(NewProcess, startVAddr, vAddrCount);

                        vAddrCount = 0;
                    }
                }

                if (vAddrCount == 0)
                {
                    startVAddr = vAddrs[i];
                }
                vAddrCount++;
            }
            if (vAddrCount > 0)
            {
                AddDataPagesToProcess(NewProcess, startVAddr, vAddrCount);
            }

            DisableKernelAccessToProcessMemory(ProcessToCopyFrom);

#if PROCESSMANAGER_TRACE
            {
                BasicConsole.WriteLine("New process memory layout:");
                BasicConsole.WriteLine(" - Code pages:");
                UInt32Dictionary.Iterator iterator = NewProcess.TheMemoryLayout.CodePages.GetNewIterator();
                while (iterator.HasNext())
                {
                    UInt32Dictionary.KeyValuePair pair = iterator.Next();
                    BasicConsole.WriteLine("VAddr: " + (Framework.String)pair.Key + " => " + pair.Value);
                }
                BasicConsole.WriteLine(" - Data pages:");
                iterator = NewProcess.TheMemoryLayout.DataPages.GetNewIterator();
                while (iterator.HasNext())
                {
                    UInt32Dictionary.KeyValuePair pair = iterator.Next();
                    BasicConsole.WriteLine("VAddr: " + (Framework.String)pair.Key + " => " + pair.Value);
                }
                BasicConsole.WriteLine(" - Kernel pages:");
                iterator = NewProcess.TheMemoryLayout.KernelPages.GetNewIterator();
                while (iterator.HasNext())
                {
                    UInt32Dictionary.KeyValuePair pair = iterator.Next();
                    BasicConsole.WriteLine("VAddr: " + (Framework.String)pair.Key + " => " + pair.Value);
                }
            }
#endif

            return NewProcess;
        }

        private static void AddCodePagesToProcess(Process NewProcess, uint startVAddr, uint vAddrCount)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.Write("Mapping free pages... Count=");
            BasicConsole.WriteLine(vAddrCount);
#endif

            void* newPAddr;
            void* newVAddr = VirtualMemoryManager.MapFreePagesForKernel(
                PageFlags.KernelOnly, (int)vAddrCount, out newPAddr);

#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Mapped.");
#endif

            NewProcess.TheMemoryLayout.AddCodePages(startVAddr, (uint)newPAddr, vAddrCount);

            CurrentProcess.TheMemoryLayout.AddDataPages((uint)newVAddr, (uint)newPAddr, vAddrCount);

            // Guarantee memory layout cleanly loaded
            //      TODO: Does this guard work properly?
            //      TODO: Is this guard even necessary?
            if (!Interrupts.Interrupts.InsideCriticalHandler)
            {
                SystemCalls.SleepThread(10);
            }

            uint* srcPtr = (uint*)startVAddr;
            uint* dstPtr = (uint*)newVAddr;
            for (uint j = 0; j < 1024*vAddrCount; j++, srcPtr++, dstPtr++)
            {
#if PROCESSMANAGER_TRACE
                if (j%1024 == 0)
                {
                    BasicConsole.WriteLine("vAddr=" + (Framework.String)((uint)srcPtr) + ", newVAddr=" + ((uint)dstPtr) + ", newPAddr=" + ((uint)newPAddr + (j * 4)));
                }
#endif

                *dstPtr = *srcPtr;
            }
            CurrentProcess.TheMemoryLayout.RemovePages((uint)newVAddr, vAddrCount);
        }

        private static void AddDataPagesToProcess(Process NewProcess, uint startVAddr, uint vAddrCount)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.Write("Mapping free pages... Count=");
            BasicConsole.WriteLine(vAddrCount);
#endif

            void* newPAddr;
            void* newVAddr = VirtualMemoryManager.MapFreePagesForKernel(
                PageFlags.KernelOnly, (int)vAddrCount, out newPAddr);

#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Mapped.");
#endif

            NewProcess.TheMemoryLayout.AddDataPages(startVAddr, (uint)newPAddr, vAddrCount);

            CurrentProcess.TheMemoryLayout.AddDataPages((uint)newVAddr, (uint)newPAddr, vAddrCount);

            // Guarantee memory layout cleanly loaded
            //      TODO: Does this guard work properly?
            //      TODO: Is this guard even necessary?
            if (!Interrupts.Interrupts.InsideCriticalHandler)
            {
                SystemCalls.SleepThread(10);
            }
            
            uint* srcPtr = (uint*)startVAddr;
            uint* dstPtr = (uint*)newVAddr;
            for (uint j = 0; j < 1024*vAddrCount; j++, srcPtr++, dstPtr++)
            {
#if PROCESSMANAGER_TRACE
                if (j%1024 == 0)
                {
                    BasicConsole.WriteLine("vAddr=" + (Framework.String)((uint)srcPtr) + ", newVAddr=" + ((uint)dstPtr) + ", newPAddr=" + ((uint)newPAddr + (j * 4)));
                }
#endif

                *dstPtr = *srcPtr;
            }
            CurrentProcess.TheMemoryLayout.RemovePages((uint)newVAddr, vAddrCount);
        }

        public static void RegisterProcess(Process process, Scheduler.Priority priority)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Registering process...");
#endif
            if (process == null)
            {
                ExceptionMethods.Throw(new Exception("Attempted to register null process!"));
            }
            else if (process.Registered)
            {
                ExceptionMethods.Throw(new Exception("Attempted to re-register process! Process name: " + process.Name));
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
                    return (Process)Processes[i];
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
        ///     Specifying threadId=-1 accepts any thread from the specified process.
        ///     No guarantees are made about the thread chosen. This is used when you
        ///     mainly want to switch process context and don't care about the specific
        ///     thread context e.g. during an interrupt.
        /// </remarks>
        [NoDebug]
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
#if PROCESSMANAGER_SWITCH_TRACE
                    BasicConsole.WriteLine("No switch. (2)");
#endif
                dontSwitchOutIn = true;
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
                //if (Scheduler.OutputMessages)
                //{
                //    BasicConsole.WriteLine("Debug Point 9.1");
                //}

                NewProcess.SwitchFromLayout(CurrentProcess != null ? CurrentProcess.TheMemoryLayout : null);

                //if (Scheduler.OutputMessages)
                //{
                //    BasicConsole.WriteLine("Debug Point 9.2");
                //}

                CurrentProcess = NewProcess;
#if PROCESSMANAGER_SWITCH_TRACE
                BasicConsole.Write("Switched in.");
#endif
            }

            CurrentThread = null;
            CurrentThread_State = null;

            //if (Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.3");
            //}

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

            //if (Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.4");
            //}

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

            //if (Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.5");
            //}

            CurrentThread_State = CurrentThread.State;

#if PROCESSMANAGER_SWITCH_TRACE
            BasicConsole.WriteLine("Thread state updated.");
#endif

            //BasicConsole.Write("Load ex state");
            ExceptionMethods.state = CurrentThread_State->ExState;

#if PROCESSMANAGER_SWITCH_TRACE
            BasicConsole.WriteLine("Exception state updated.");

            if (Scheduler.OutputMessages)
            {
                BasicConsole.Write("Switch to ");
                BasicConsole.Write(CurrentProcess.Name);
                BasicConsole.Write(" : ");
                BasicConsole.WriteLine(CurrentThread.Name);
            }
#endif
        }

        public static bool WakeThread(uint processId, uint threadId)
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

            if (theSemaphore == null)
            {
                result = Semaphores.Count;
                theSemaphore = new Semaphore(result, limit);
                Semaphores.Add(theSemaphore);
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

        public static int Semaphore_WaitCurrentThread(int id)
        {
            if (Semaphore_VerifyOwner(id, CurrentProcess))
            {
                ((Semaphore)Semaphores[id]).Wait();
                return 1;
            }
            return -1;
        }

        public static int Semaphore_Wait(int id, Process aProcess, Thread aThread)
        {
            if (Semaphore_VerifyOwner(id, aProcess))
            {
                return ((Semaphore)Semaphores[id]).WaitOnBehalf(aProcess, aThread) ? 1 : 0;
            }
            else
            {
                BasicConsole.WriteLine("Error! Semaphore_VerifyOwner failed: ProcessId: " + (String)aProcess.Id + "ThreadId: " + aThread.Id + ", SemaphoreId: " + id);
            }
            return -1;
        }

        public static bool Semaphore_Signal(int id, Process aProcess)
        {
            //BasicConsole.WriteLine("Semaphore_Signal : Verifying...");
            if (Semaphore_VerifyOwner(id, aProcess))
            {
                //BasicConsole.WriteLine("Semaphore_Signal : Passed verification. Signalling...");

                ((Semaphore)Semaphores[id]).SignalOnBehalf();

                //BasicConsole.WriteLine("Semaphore_Signal : Signalled.");

                return true;
            }
            else
            {
                BasicConsole.WriteLine("Error! Semaphore_VerifyOwner failed: ProcessId: " + (String)aProcess.Id + ", SemaphoreId: " + id);
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
                Semaphore theSemaphore = (Semaphore)Semaphores[id];
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

        public static void* EnableDebuggerAccessToProcessMemory(Process TargetProcess, void* PageToShadowPAddr)
        {
            if (ShadowPageVAddr == (void*)0xFFFFFFFF)
            {
                ShadowPageVAddr =
                    VirtualMemoryManager.MapFreePageForKernel(PageFlags.KernelOnly,
                        out ShadowPagePAddr);
                KernelProcess.TheMemoryLayout.AddKernelPage((uint)ShadowPagePAddr, (uint)ShadowPageVAddr);
            }

            KernelProcess.TheMemoryLayout.ReplaceKernelPage((uint)ShadowPageVAddr, (uint)PageToShadowPAddr);
            return ShadowPageVAddr;
        }

        public static void DisableDebuggerAccessToProcessMemory(Process TargetProcess)
        {
            KernelProcess.TheMemoryLayout.ReplaceKernelPage((uint)ShadowPageVAddr, (uint)ShadowPagePAddr);
        }
    }
}