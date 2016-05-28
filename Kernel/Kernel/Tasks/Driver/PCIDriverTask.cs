using Kernel.Consoles;
using Kernel.PCI;

namespace Kernel.Tasks.Driver
{
    public static class PCIDriverTask
    {
        private static VirtualConsole console;

        private static uint GCThreadId;

        public static void Main()
        {
            Helpers.ProcessInit("PCI Driver", out GCThreadId);

            try
            {
                BasicConsole.WriteLine("PCI Driver > Creating virtual console...");
                console = new VirtualConsole();

                BasicConsole.WriteLine("PCI Driver > Connecting virtual console...");
                console.Connect();

                BasicConsole.WriteLine("PCI Driver > Executing.");

                try
                {
                    BasicConsole.WriteLine("PCI Driver > Initialising PCI Manager...");
                    PCIManager.Init();

                    BasicConsole.WriteLine("PCI Driver > Enumerating PCI devices...");
                    PCIManager.EnumerateDevices();

                    BasicConsole.WriteLine("PCI Driver > Outputting PCI info...");
                    OutputPCI();
                }
                catch
                {
                    BasicConsole.WriteLine("PCI Driver > Error executing!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }

                BasicConsole.WriteLine("PCI Driver > Execution complete.");
            }
            catch
            {
                BasicConsole.WriteLine("PCI Driver > Error initialising!");
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            }

            BasicConsole.WriteLine("PCI Driver > Exiting...");
        }

        /// <summary>
        ///     Outputs the PCI system information.
        /// </summary>
        private static void OutputPCI()
        {
            for (int i = 0; i < PCIManager.Devices.Count; i++)
            {
                PCIDevice aDevice = (PCIDevice) PCIManager.Devices[i];
                console.WriteLine(PCIDevice.DeviceClassInfo.GetString(aDevice));
                console.Write(" - Address: ");
                console.Write(aDevice.bus);
                console.Write(":");
                console.Write(aDevice.slot);
                console.Write(":");
                console.WriteLine(aDevice.function);

                console.Write(" - Vendor Id: ");
                console.WriteLine(aDevice.VendorID);

                console.Write(" - Device Id: ");
                console.WriteLine(aDevice.DeviceID);
            }
        }
    }
}