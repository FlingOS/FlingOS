using System;
using Kernel.FOS_System;
using Kernel.Core.Processes;

namespace Kernel.Core.Tasks
{
    public static unsafe class DeviceManagerTask
    {
        public static bool Terminating = false;

        private static uint GCThreadId;

        private static Pipes.Standard.StandardOutpoint StdOut;
        private static Pipes.Standard.StandardInpoint StdIn;

        public static void Main()
        {
            BasicConsole.WriteLine("Device Manager started.");

            Hardware.Processes.ProcessManager.CurrentProcess.InitHeap();
            SystemCallResults SysCallResult = SystemCallMethods.StartThread(GCCleanupTask.Main, out GCThreadId);
            if (SysCallResult != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Device Manager: GC thread failed to create!");
            }

            try
            {
                StdOut = new Pipes.Standard.StandardOutpoint(true);
                int StdOutPipeId = StdOut.WaitForConnect();

                int numOutpoints;
                Pipes.BasicServerHelpers.GetNumPipeOutpoints(out numOutpoints, out SysCallResult, Pipes.PipeClasses.Standard, Pipes.PipeSubclasses.Standard_In);
                if (SysCallResult == SystemCallResults.OK && numOutpoints > 0)
                {
                    Pipes.PipeOutpointDescriptor[] OutpointDescriptors;
                    Pipes.BasicServerHelpers.GetOutpointDescriptors(numOutpoints, ref SysCallResult, out OutpointDescriptors, Pipes.PipeClasses.Standard, Pipes.PipeSubclasses.Standard_In);

                    if (SysCallResult == SystemCallResults.OK)
                    {
                        for (int i = 0; i < OutpointDescriptors.Length; i++)
                        {
                            Pipes.PipeOutpointDescriptor Descriptor = OutpointDescriptors[i];
                            StdIn = new Pipes.Standard.StandardInpoint(Descriptor.ProcessId, false);
                        }
                    }
                }

                uint loops = 0;
                while (!Terminating)
                {
                    try
                    {
                        StdOut.Write(StdOutPipeId, "Hello, world! (" + (FOS_System.String)loops++ + ")\n", true);
                    }
                    catch
                    {
                        BasicConsole.WriteLine("DM > Error writing to StdOut!");
                        BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    }

                    SystemCallMethods.SleepThread(1000);
                }
            }
            catch
            {
                BasicConsole.WriteLine("DM > Error initialising!");
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            }
        }
    }
}
