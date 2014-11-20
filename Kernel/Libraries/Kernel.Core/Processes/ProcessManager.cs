#define PROCESSMANAGER_TRACE
#undef PROCESSMANAGER_TRACE

using System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.IO;

namespace Kernel.Core.Processes
{
    public static unsafe class ProcessManager
    {
        public static List Processes = new List();
        public static Process CurrentProcess = null;
        public static Thread CurrentThread = null;
        public static ThreadState* CurrentThread_State = null;

        private static uint ProcessIdGenerator = 0;

        public static Process LoadSampleProcess(bool UserMode)
        {
#if PROCESSMANAGER_TRACE
            Console.Default.WriteLine(" > Creating sample process object...");
#endif
            return CreateProcess(SampleProcess.Main, "Sample Process", UserMode);
        }
        public static Process LoadProcess_FromRawExe(File RawExeFile, bool UserMode)
        {
            // - Read in file contents
            // - Map enough memory for the exe file contents
            // - Copy the memory over

            //TODO - Handle case of EXE being bigger than 4KiB?
            //          - Would need to map contiguous pages.
            byte* destMemPtr = (byte*)Hardware.VirtMemManager.MapFreePage(
                UserMode ? Hardware.VirtMem.VirtMemImpl.PageFlags.None : Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly);

            int bytesRead = 0;
            byte[] readBuffer = new byte[4096];
            bytesRead = RawExeFile.GetStream().Read(readBuffer, 0, 4096);
            Utilities.MemoryUtils.MemCpy_32(destMemPtr, ((byte*)Utilities.ObjectUtilities.GetHandle(readBuffer)) + FOS_System.Array.FieldsBytesSize, (uint)bytesRead);
            
            // - Create the process
            return CreateProcess((ThreadStartMethod)Utilities.ObjectUtilities.GetObject(destMemPtr), RawExeFile.Name, UserMode);                                          
        }
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
    }
}
