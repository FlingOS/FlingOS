using Kernel.Consoles;
using Kernel.FOS_System;
using Kernel.FOS_System.Processes;
using Kernel.FOS_System.Processes.Requests.Processes;
using Kernel.Hardware.Processes;

namespace Kernel.Tasks.App
{
    public static class SystemStatusTask
    {
        private static VirtualConsole console;

        private static uint GCThreadId;

        private static readonly bool Terminating = false;

        public static void Main()
        {
            Helpers.ProcessInit("System Status App", out GCThreadId);

            try
            {
                BasicConsole.WriteLine("System Status > Creating virtual console...");
                console = new VirtualConsole();

                BasicConsole.WriteLine("System Status > Connecting virtual console...");
                console.Connect();

                BasicConsole.WriteLine("System Status > Executing.");

                try
                {
                    while (!Terminating)
                    {
                        int NumProcesses;
                        if (SystemCalls.GetNumProcesses(out NumProcesses) == SystemCallResults.OK)
                        {
                            console.WriteLine("Number of processes: " + Int32.ToDecimalString(NumProcesses));

                            unsafe
                            {
                                ProcessDescriptor* ProcessList =
                                    (ProcessDescriptor*)
                                        Heap.AllocZeroed((uint) (sizeof(ProcessDescriptor)*NumProcesses),
                                            "System Status Task : Main : ProcessList");
                                try
                                {
                                    if (SystemCalls.GetProcessList(ProcessList, NumProcesses) == SystemCallResults.OK)
                                    {
                                        for (int i = 0; i < NumProcesses; i++)
                                        {
                                            ProcessDescriptor* descriptor = ProcessList + i;
                                            console.Write_AsDecimal(descriptor->Id);
                                            console.Write(":");

                                            switch ((Scheduler.Priority) descriptor->Priority)
                                            {
                                                case Scheduler.Priority.High:
                                                    console.Write("High");
                                                    break;
                                                case Scheduler.Priority.Low:
                                                    console.Write("Low");
                                                    break;
                                                case Scheduler.Priority.Normal:
                                                    console.Write("Normal");
                                                    break;
                                                case Scheduler.Priority.ZeroTimed:
                                                    console.Write("Zero Timed");
                                                    break;
                                            }

                                            if (i < NumProcesses - 1)
                                            {
                                                console.Write(", ");
                                            }
                                        }
                                        console.WriteLine();
                                    }
                                    else
                                    {
                                        console.WriteLine("Could not get process list!");
                                    }
                                }
                                finally
                                {
                                    Heap.Free(ProcessList);
                                }
                            }
                        }
                        else
                        {
                            console.WriteLine("Could not get number of processes!");
                        }


                        int FSCount;
                        if (SystemCalls.StatFS(out FSCount) == SystemCallResults.OK)
                        {
                            console.WriteLine("Number of file systems: " + Int32.ToDecimalString(FSCount));

                            if (FSCount > 0)
                            {
                                unsafe
                                {
                                    char* MappingsPtr =
                                        (char*)
                                            Heap.AllocZeroed((uint) (sizeof(char)*FSCount*10),
                                                "System Status Task : Main : Mappings");
                                    try
                                    {
                                        uint* ProcessesPtr =
                                            (uint*)
                                                Heap.AllocZeroed((uint) (sizeof(uint)*FSCount),
                                                    "System Status Task : Main : FS Processes");
                                        if (SystemCalls.StatFS(ref FSCount, MappingsPtr, ProcessesPtr) ==
                                            SystemCallResults.OK)
                                        {
                                            String[] Mappings = new String[FSCount];
                                            for (int i = 0; i < FSCount; i++)
                                            {
                                                Mappings[i] = ByteConverter.GetASCIIStringFromUTF16(
                                                    (byte*) MappingsPtr, (uint) (i*10*sizeof(char)), 10);
                                                console.Write(Mappings[i] + "@" + ProcessesPtr[i]);
                                                if (i < FSCount - 1)
                                                {
                                                    console.Write(", ");
                                                }
                                            }
                                            console.WriteLine();
                                        }
                                        else
                                        {
                                            console.WriteLine("Could not get file system mappings!");
                                        }
                                    }
                                    finally
                                    {
                                        Heap.Free(MappingsPtr);
                                    }
                                }
                            }
                        }
                        else
                        {
                            console.WriteLine("Could not get number of file systems!");
                        }

                        SystemCalls.SleepThread(10000);
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("System Status > Error executing!");
                    if (ExceptionMethods.CurrentException != null)
                    {
                        BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    }
                    else
                    {
                        BasicConsole.WriteLine("Error is null!");
                    }
                }

                BasicConsole.WriteLine("System Status > Execution complete.");
            }
            catch
            {
                BasicConsole.WriteLine("System Status > Error initialising!");
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            }

            BasicConsole.WriteLine("System Status > Exiting...");
        }
    }
}