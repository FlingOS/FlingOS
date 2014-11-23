#define PROCESSMANAGER_TRACE
#undef PROCESSMANAGER_TRACE

using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.Processes
{
    public static unsafe class ProcessManager
    {
        public static List Processes = new List();
        public static Process CurrentProcess = null;
        public static Thread CurrentThread = null;
        public static ThreadState* CurrentThread_State = null;

        private static uint ProcessIdGenerator = 0;

        public static Process CreateProcess(ThreadStartMethod MainMethod, FOS_System.String Name, bool UserMode)
        {
#if PROCESSMANAGER_TRACE
            Console.Default.WriteLine(" > Creating process object...");
#endif
            return new Process(MainMethod, ProcessIdGenerator++, Name, UserMode);
        }
        public static void RegisterProcess(Process process, Scheduler.Priority priority)
        {
#if PROCESSMANAGER_TRACE
            Console.Default.WriteLine(" > > Adding process...");
#endif
            Scheduler.InitProcess(process, priority);

            Processes.Add(process);
        }

        public static void SwitchProcess(uint processId, int threadId)
        {
            //Switch the current memory layout across.
            //  Don't touch register state etc, just the memory layout

            if (CurrentProcess != null)
            {
                CurrentProcess.SwitchOut();
            }
            else if (CurrentProcess.Id == processId &&
                     CurrentThread.Id == threadId)
            {
                return;
            }

            for (int i = 0; i < Processes.Count; i++)
            {
                if (((Process)Processes[i]).Id == processId)
                {
                    CurrentProcess = ((Process)Processes[i]);
                    break;
                }
            }

            if (threadId == -1)
            {
                CurrentThread = (Thread)CurrentProcess.Threads[0];
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

            CurrentThread_State = CurrentThread.State;

            CurrentProcess.SwitchIn();
        }
    }
}
