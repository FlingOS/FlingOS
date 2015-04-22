
using System;
using Kernel.Hardware.Processes;
using Kernel.Hardware.Interrupts;

namespace Kernel.Hardware.Tasks
{
    public static class DeviceManagerTask
    {
        public static Thread OwnerThread = null;
        public static bool Awake = true;

        public static bool Terminate = false;

        public static void Main()
        {
            OwnerThread = ProcessManager.CurrentThread;

            Thread.Sleep_Indefinitely();

            while (!Terminate)
            {
                Awake = false;

                DeviceManager.UpdateDevices();
                
                if (!Awake)
                {
                    if (!Thread.Sleep_Indefinitely())
                    {
                        BasicConsole.SetTextColour(BasicConsole.error_colour);
                        BasicConsole.WriteLine("Failed to sleep device manager thread!");
                        BasicConsole.SetTextColour(BasicConsole.default_colour);
                    }
                }
            }
        }
    }
}
