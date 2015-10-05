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

            //FOS_System.GC.Init();
            //if (SystemCallMethods.CreateThread(GCCleanupTask.Main) != SystemCallResults.OK)
            //{
            //    BasicConsole.WriteLine("Device Manager: GC thread failed to create!");
            //}

            while (!Terminating)
            {
                //TODO

                SystemCallMethods.Sleep(10000);
            }
        }
    }

    class testclass : FOS_System.Object
    {
    }
}
