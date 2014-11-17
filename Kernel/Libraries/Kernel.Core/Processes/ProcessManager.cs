#define PROCESSMANAGER_TRACE
#undef PROCESSMANAGER_TRACE

using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Core.Processes
{
    public static unsafe class ProcessManager
    {
        public static List Processes = new List();
        public static Process CurrentProcess = null;
        public static Thread CurrentThread = null;
        public static ThreadState* CurrentThread_State = null;

        private static uint ProcessIdGenerator = 0;

        public static Process LoadSampleProcess()
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine(" > Creating sample process object...");
#endif
            return CreateProcess(SampleProcess.GetMainMethodPtr(), "Sample Process");
        }
        public static Process CreateProcess(void* MainMethodPtr, FOS_System.String Name)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine(" > Creating process object...");
#endif
            return new Process(MainMethodPtr, ProcessIdGenerator++, Name);
        }
        public static void RegisterProcess(Process process, Scheduler.Priority priority)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine(" > > Adding process...");
#endif
            Scheduler.InitProcess(process, priority);

            Processes.Add(process);
        }
    }
}
