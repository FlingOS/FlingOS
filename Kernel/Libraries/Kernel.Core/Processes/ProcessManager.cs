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
            BasicConsole.WriteLine(" > Creating sample process object...");
            return CreateProcess(SampleProcess.GetMainMethodPtr(), "Sample Process");
        }
        public static Process CreateProcess(void* MainMethodPtr, FOS_System.String Name)
        {
            BasicConsole.WriteLine(" > Creating process object...");
            return new Process(MainMethodPtr, ProcessIdGenerator++, Name);
        }
        public static void RegisterProcess(Process process)
        {
            BasicConsole.WriteLine(" > > Adding process...");
            Processes.Add(process);
        }
    }
}
