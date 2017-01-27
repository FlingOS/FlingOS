using Kernel.Consoles;
using Kernel.FileSystems;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Processes;
using Kernel.Multiprocessing;
using Int32 = Kernel.Framework.Int32;
using String = Kernel.Framework.String;

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
                    List FSAccessors = new List();
                    /* Note: 
                     *  System Status Task is started by Kernel Task after Kernel Task has initialised its file system stuff.
                     *  As a result, the unclean state is copied to System Status Task. The call to Hard Reset cleans up the 
                     *  state ready for use below (without adverse effects on the GC).
                     */
                    FileSystemAccessor.HardReset();

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
                                        Heap.AllocZeroed((uint)(sizeof(ProcessDescriptor)*NumProcesses),
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

                                            switch ((Scheduler.Priority)descriptor->Priority)
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

                        String[] Mappings;
                        uint[] ProcessIds;
                        if (FileSystemAccessor.StatFSBySysCalls(out Mappings, out ProcessIds))
                        {
                            for (int i = 0; i < ProcessIds.Length; i++)
                            {
                                bool Found = false;
                                for (int j = 0; j < FSAccessors.Count && !Found; j++)
                                {
                                    Found = ((FileSystemAccessor)FSAccessors[j]).RemoteProcessId == ProcessIds[i];
                                }
                                if (!Found)
                                {
                                    FileSystemAccessor NewAccessor = new FileSystemAccessor(ProcessIds[i]);
                                    FSAccessors.Add(NewAccessor);
                                }
                            }
                        }
                        else
                        {
                            BasicConsole.WriteLine("System Status > Unable to retrieve file system information. StatFS by system calls failed.");
                            console.WriteLine("Unable to retrieve file system information. StatFS by system calls failed.");
                        }

                        for (int i = 0; i < FSAccessors.Count; i++)
                        {
                            FileSystemAccessor Accessor = ((FileSystemAccessor)FSAccessors[i]);;
                            Accessor.StatFS();

                            for (int j = 0; j < Accessor.MappingPrefixes.Length; j++)
                            {
                                String Mapping = Accessor.MappingPrefixes[j];
                                List Dirs = Accessor.ListDir(Mapping);

                                for (int k = 0; k < Dirs.Count; k++)
                                {
                                    String Dir = (String)Dirs[k];
                                    console.WriteLine(Mapping + Dir);
                                }
                                if (Dirs.Count == 0)
                                {
                                    console.WriteLine(Mapping + " IS AN EMPTY DRIVE.");
                                }
                            }
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
                BasicConsole.WriteLine(ExceptionMethods.CurrentException != null
                    ? ExceptionMethods.CurrentException.Message
                    : "[NO EXCEPTION OBJECT]");
            }

            BasicConsole.WriteLine("System Status > Exiting...");
        }
    }
}