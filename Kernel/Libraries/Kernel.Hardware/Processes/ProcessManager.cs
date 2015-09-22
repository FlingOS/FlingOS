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
    
#define PROCESSMANAGER_TRACE
#undef PROCESSMANAGER_TRACE

#define PROCESSMANAGER_SWITCH_TRACE
#undef PROCESSMANAGER_SWITCH_TRACE

using System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.Processes.Synchronisation;
using Kernel.Hardware.Processes.Synchronisation;

namespace Kernel.Hardware.Processes
{
    public static unsafe class ProcessManager
    {
        public static List Processes = new List();
        public static Process CurrentProcess = null;
        public static Thread CurrentThread = null;
        public static ThreadState* CurrentThread_State = null;

        private static uint ProcessIdGenerator = 1;

        private static List Semaphores = new List(1024, 1024);
        private static SpinLock SemaphoresLock = new SpinLock(-1);

        public static Process CreateProcess(ThreadStartMethod MainMethod, FOS_System.String Name, bool UserMode)
        {
            return CreateProcess(MainMethod, Name, UserMode, false);
        }
        public static Process CreateProcess(ThreadStartMethod MainMethod, FOS_System.String Name, bool UserMode, bool CreateHeap)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Creating process object...");
#endif
            return new Process(MainMethod, ProcessIdGenerator++, Name, UserMode, CreateHeap);
        }
        public static void RegisterProcess(Process process, Scheduler.Priority priority)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Registering process...");
            BasicConsole.WriteLine("Disabling scheduler...");
#endif
            if (process == null)
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Attempted to register null process!"));
            }

            //bool reenable = Scheduler.Enabled;
            //if (reenable)
            //{
            //    Scheduler.Disable();
            //}
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Initialising process...");
#endif
            Scheduler.InitProcess(process, priority);

#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Adding process...");
#endif

            Processes.Add(process);

#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Enabling scheduler...");
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
        public static void SwitchProcess(uint processId, int threadId)
        {
            //Switch the current memory layout across.
            //  Don't touch register state etc, just the memory layout

            bool dontSwitchOutIn = false;

            if (CurrentProcess != null &&
                CurrentProcess.Id == processId)
            {
                if (CurrentThread != null &&
                    (CurrentThread.Id == threadId || threadId == -1))
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
                CurrentProcess.UnloadHeap();
                CurrentProcess.UnloadMemLayout();

                CurrentProcess = GetProcessById(processId);

                // Process not found
                if (CurrentProcess == null)
                {
#if PROCESSMANAGER_SWITCH_TRACE
                    BasicConsole.WriteLine("Process not found.");
#endif
                    return;
                }

#if PROCESSMANAGER_SWITCH_TRACE
                BasicConsole.Write("Switching in: ");
                BasicConsole.WriteLine(CurrentProcess.Name);
#endif
                CurrentProcess.LoadMemLayout();
                CurrentProcess.LoadHeap();
            }

            CurrentThread = null;
            CurrentThread_State = null;

            if (threadId == -1)
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
            bool Woken = false;
            Process theProcess = null;
            
            for (int i = 0; i < Processes.Count; i++)
            {
                Process aProcess = ((Process)Processes[i]);
                if (aProcess.Id == processId)
                {
                    theProcess = aProcess;
                }
            }

            if (theProcess != null)
            {
                for (int i = 0; i < theProcess.Threads.Count; i++)
                {
                    Thread aThread = ((Thread)theProcess.Threads[i]);
                    if (aThread.Id == threadId)
                    {
                        aThread._Wake();
                        Woken = true;
                    }
                }
            }

            return Woken;
        }

        public static int Semaphore_Allocate(int limit, Process aProcess)
        {
            int result = -1;
            Semaphore theSemaphore = null;

            SemaphoresLock.Enter();

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

            SemaphoresLock.Exit();

            theSemaphore.OwnerProcesses.Add(aProcess.Id);

            return result;
        }
        public static bool Semaphore_Deallocate(int id, Process aProcess)
        {
            if (Semaphore_VerifyOwner(id, aProcess))
            {
                SemaphoresLock.Enter();
                Semaphores[id] = null;
                SemaphoresLock.Exit();

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
        public static bool Semaphore_AddOwner(int semaphoreId, uint processId, Process aProcess)
        {
            if (Semaphore_VerifyOwner(semaphoreId, aProcess))
            {
                ((Semaphore)Semaphores[semaphoreId]).OwnerProcesses.Add(processId);
                return true;
            }
            return false;
        }
        private static bool Semaphore_VerifyOwner(int id, Process aProcess)
        {
            if (id > -1 && id < Semaphores.Count && Semaphores[id] != null)
            {
                Semaphore theSemaphore = ((Semaphore)Semaphores[id]);
                return theSemaphore.OwnerProcesses.IndexOf(aProcess.Id) >= 0;
            }
            return false;
        }
    }
}
