using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Core.Processes
{
    public static unsafe class ProcessManager
    {
        public static List Processes = new List();
        public static Process CurrentProcess = null;

        private static uint ProcessIdGenerator = 0;

        public static Process LoadSampleProcess()
        {
            return CreateProcess(SampleProcess.GetMainMethodPtr(), "Sample Process");
        }
        public static Process CreateProcess(void* MainMethodPtr, FOS_System.String Name)
        {
            return new Process(MainMethodPtr, ProcessIdGenerator++, Name);
        }
        public static void RegisterProcess(Process process)
        {
            Processes.Add(process);
        }
    }
}
