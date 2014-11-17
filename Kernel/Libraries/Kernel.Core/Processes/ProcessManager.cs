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
            Console.Default.WriteLine(" > Creating sample process object...");
#endif
            return CreateProcess(SampleProcess.Main, "Sample Process");
        }
        public static Process CreateProcess(ThreadStartMethod MainMethod, FOS_System.String Name)
        {
#if PROCESSMANAGER_TRACE
            Console.Default.WriteLine(" > Creating process object...");
#endif
            return new Process(MainMethod, ProcessIdGenerator++, Name);
        }
        public static void RegisterProcess(Process process, Scheduler.Priority priority)
        {
#if PROCESSMANAGER_TRACE
            Console.Default.WriteLine(" > > Adding process...");
#endif
            Scheduler.InitProcess(process, priority);

            Processes.Add(process);
        }
    }
}
