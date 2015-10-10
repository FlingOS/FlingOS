using System;
using Kernel.FOS_System;
using Kernel.Core.Processes;

namespace Kernel.Core.Tasks
{
    public static unsafe class DeviceManagerTask
    {
        public static bool Terminating = false;

        private static uint GCThreadId;

        private static Pipes.Standard.StandardOutPipe StdOut;

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
                StdOut = new Pipes.Standard.StandardOutPipe();
                StdOut.WaitForConnect();

                uint loops = 0;
                while (!Terminating)
                {
                    try
                    {
                        StdOut.Write("Hello, world! (" + (FOS_System.String)loops++ + ")\n");
                    }
                    catch
                    {
                        BasicConsole.WriteLine("DM > Error writing to StdOut!");
                        BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    }

                    //SystemCallMethods.SleepThread(1000);
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
