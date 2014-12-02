#define PROCESSMANAGER_TRACE
#undef PROCESSMANAGER_TRACE

using System;
using Kernel.FOS_System.Collections;
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

        public static Process CreateProcess(ThreadStartMethod MainMethod, FOS_System.String Name, bool UserMode)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Creating process object...");
#endif
            return new Process(MainMethod, ProcessIdGenerator++, Name, UserMode);
        }
        public static void RegisterProcess(Process process, Scheduler.Priority priority)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Registering process...");
            BasicConsole.WriteLine("Disabling scheduler...");
#endif
            if (process == null)
            {
                return;
            }

            bool reenable = Scheduler.Enabled;
            if (reenable)
            {
                Scheduler.Disable();
            }
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
            if (reenable)
            {
                Scheduler.Enable();
            }
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
                    return;
                }
                else
                {
                    dontSwitchOutIn = true;
                }
            }

            if (!dontSwitchOutIn)
            {
                //BasicConsole.WriteLine("Switching out: " + CurrentProcess.Name);
                CurrentProcess.SwitchOut();

                CurrentProcess = null;
                
                for (int i = 0; i < Processes.Count; i++)
                {
                    if (((Process)Processes[i]).Id == processId)
                    {
                        CurrentProcess = ((Process)Processes[i]);
                        break;
                    }
                }

                // Process not found
                if (CurrentProcess == null)
                {
                    BasicConsole.WriteLine("Process not found.");
                    return;
                }
                //BasicConsole.WriteLine("Process found. " + CurrentProcess.Name);
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
                for (int i = 0; i < CurrentProcess.Threads.Count; i++)
                {
                    if (((Thread)CurrentProcess.Threads[i]).Id == threadId)
                    {
                        CurrentThread = (Thread)CurrentProcess.Threads[i];
                        break;
                    }
                }
            }

            // No threads in the process (?!) or process not found
            if (CurrentThread == null)
            {
                BasicConsole.WriteLine("Thread not found.");
                return;
            }
            //BasicConsole.WriteLine("Thread found.");

            CurrentThread_State = CurrentThread.State;
            //BasicConsole.WriteLine("Thread state updated.");
            
            if (!dontSwitchOutIn)
            {
                //BasicConsole.WriteLine("Switching in: " + CurrentProcess.Name);
                CurrentProcess.SwitchIn();
            }
        }

        #region Locks

        public static List SpinLocks = new List();
        public static List Semaphores = new List();
        public static List Mutexes = new List();

        private static SpinLock SpinLocks_Lock = new SpinLock(0);
        private static SpinLock Semaphores_Lock = new SpinLock(0);
        private static SpinLock Mutexes_Lock = new SpinLock(0);

        private static int SpinLocks_IdGenerator = 1;
        private static int Semaphores_IdGenerator = 1;
        private static int Mutexes_IdGenerator = 1;

        public static int CreateSpinLock()
        {
            SpinLock newLock = new SpinLock(SpinLocks_IdGenerator++);
            SpinLocks.Add(newLock);
            return newLock.Id;
        }
        public static int CreateSemaphore(int limit)
        {
            Semaphore newLock = new Semaphore(Semaphores_IdGenerator++, limit);
            Semaphores.Add(newLock);
            return newLock.Id;
        }
        public static int CreateMutex()
        {
            Mutex newLock = new Mutex(Mutexes_IdGenerator++);
            Mutexes.Add(newLock);
            return newLock.Id;
        }

        public static int CreateSpinLock_Safe()
        {
            SpinLocks_Lock.Enter();
            int result = CreateSpinLock();
            SpinLocks_Lock.Exit();
            return result;
        }
        public static int CreateSemaphore_Safe(int limit)
        {
            Semaphores_Lock.Enter();
            int result = CreateSemaphore(limit);
            Semaphores_Lock.Exit();
            return result;
        }
        public static int CreateMutex_Safe()
        {
            Mutexes_Lock.Enter();
            int result = CreateMutex();
            Mutexes_Lock.Exit();
            return result;
        }

        public static void DestroySpinLock(int id)
        {
            for (int i = 0; i < SpinLocks.Count; i++)
            {
                SpinLock aLock = (SpinLock)SpinLocks[i];
                if (aLock.Id == id)
                {
                    SpinLocks.RemoveAt(i);
                    break;
                }
            }
        }
        public static void DestroySemaphore(int id)
        {
            for (int i = 0; i < Semaphores.Count; i++)
            {
                Semaphore aLock = (Semaphore)Semaphores[i];
                if (aLock.Id == id)
                {
                    Semaphores.RemoveAt(i);
                    break;
                }
            }
        }
        public static void DestroyMutex(int id)
        {
            for (int i = 0; i < Mutexes.Count; i++)
            {
                Mutex aLock = (Mutex)Mutexes[i];
                if (aLock.Id == id)
                {
                    Mutexes.RemoveAt(i);
                    break;
                }
            }
        }
        
        public static void DestroySpinLock_Safe(int id)
        {
            SpinLocks_Lock.Enter();
            DestroySpinLock(id);
            SpinLocks_Lock.Exit();
        }
        public static void DestroySemaphore_Safe(int id)
        {
            Semaphores_Lock.Enter();
            DestroySemaphore(id);
            Semaphores_Lock.Exit();
        }
        public static void DestroyMutex_Safe(int id)
        {
            Mutexes_Lock.Enter();
            DestroyMutex(id);
            Mutexes_Lock.Exit();
        }

        public static void EnterSpinLock(int id)
        {
            for (int i = 0; i < SpinLocks.Count; i++)
            {
                SpinLock aLock = (SpinLock)SpinLocks[i];
                if (aLock.Id == id)
                {
                    aLock.Enter();
                    break;
                }
            }
        }
        public static void ExitSpinLock(int id)
        {
            for (int i = 0; i < SpinLocks.Count; i++)
            {
                SpinLock aLock = (SpinLock)SpinLocks[i];
                if (aLock.Id == id)
                {
                    aLock.Exit();
                    break;
                }
            }
        }

        public static bool WaitSemaphore(int id)
        {
            for (int i = 0; i < Semaphores.Count; i++)
            {
                Semaphore aLock = (Semaphore)Semaphores[i];
                if (aLock.Id == id)
                {
                    return aLock.Wait();
                }
            }
            return false;
        }
        public static void SignalSemaphore(int id)
        {
            for (int i = 0; i < Semaphores.Count; i++)
            {
                Semaphore aLock = (Semaphore)Semaphores[i];
                if (aLock.Id == id)
                {
                    aLock.Signal();
                    break;
                }
            }
        }

        public static bool WaitMutex(int id)
        {
            for (int i = 0; i < Mutexes.Count; i++)
            {
                Mutex aLock = (Mutex)Mutexes[i];
                if (aLock.Id == id)
                {
                    return aLock.Wait();
                }
            }
            return false;
        }
        public static void SignalMutex(int id)
        {
            for (int i = 0; i < Mutexes.Count; i++)
            {
                Mutex aLock = (Mutex)Mutexes[i];
                if (aLock.Id == id)
                {
                    aLock.Signal();
                    break;
                }
            }
        }

        #endregion
    }
}
