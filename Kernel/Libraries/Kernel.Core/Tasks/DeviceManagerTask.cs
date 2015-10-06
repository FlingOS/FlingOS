using System;
using Kernel.FOS_System;
using Kernel.Core.Processes;

namespace Kernel.Core.Tasks
{
    public static unsafe class DeviceManagerTask
    {
        public static bool Terminating = false;

        public static void Main()
        {
            BasicConsole.WriteLine("Device Manager started.");

            Hardware.Processes.ProcessManager.CurrentProcess.InitHeap();
            if (SystemCallMethods.CreateThread(GCCleanupTask.Main) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: GC thread failed to create!");
            }

            while (!Terminating)
            {
                //TODO
                testclass x = new testclass();

                SystemCallMethods.Sleep(10000);
            }
        }
    }

    class testclass : FOS_System.Object
    {
    }
}
