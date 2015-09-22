using System;
using Kernel.FOS_System;
using Kernel.Core.Processes;

namespace Kernel.Core.Tasks
{
    public static unsafe class DeviceManagerTask
    {
        public static void Main()
        {
            BasicConsole.WriteLine("Device Manager started.");

            while (true)
            {
                //TODO

                SystemCallMethods.Sleep(-1);
            }
        }
    }
}
