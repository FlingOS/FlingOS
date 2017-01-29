using Kernel.ATA;
using Kernel.Consoles;
using Kernel.Devices;
using Kernel.Framework;
using Kernel.Framework.Processes;

namespace Kernel.Tasks.Driver
{
    public static class ATADriverTask
    {
        private static VirtualConsole console;

        private static uint GCThreadId;

        public static void Main()
        {
            Helpers.ProcessInit("ATA Driver", out GCThreadId);

            try
            {
                BasicConsole.WriteLine("ATA Driver > Creating virtual console...");
                console = new VirtualConsole();

                BasicConsole.WriteLine("ATA Driver > Connecting virtual console...");
                console.Connect();

                BasicConsole.WriteLine("ATA Driver > Registering for IRQs...");
                SystemCalls.RegisterIRQHandler(14, IRQHandler);
                SystemCalls.RegisterIRQHandler(15);

                BasicConsole.WriteLine("ATA Driver > Executing.");

                DeviceManager.InitForProcess();

                try
                {
                    BasicConsole.WriteLine("ATA Driver > Initialising ATA Manager...");
                    ATAManager.Init();

                    BasicConsole.WriteLine("ATA Driver > Outputting ATA info...");
                    OutputATA();
                }
                catch
                {
                    BasicConsole.WriteLine("ATA Driver > Error executing!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }

                BasicConsole.WriteLine("ATA Driver > Execution complete.");
            }
            catch
            {
                BasicConsole.WriteLine("ATA Driver > Error initialising!");
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            }

            BasicConsole.WriteLine("ATA Driver > Exiting...");
        }

        /// <summary>
        ///     Outputs the ATA system information.
        /// </summary>
        private static void OutputATA()
        {
            int numDrives = 0;
            for (int i = 0; i < ATAManager.Devices.Count; i++)
            {
                Device aDevice = (Device)ATAManager.Devices[i];
                if (aDevice is PATA)
                {
                    console.WriteLine();
                    console.Write("--------------------- Device ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");
                    console.WriteLine("Type: PATA");

                    PATA theATA = (PATA)aDevice;
                    console.WriteLine("Serial No: " + theATA.SerialNo);
                    console.WriteLine("Firmware Rev: " + theATA.FirmwareRev);
                    console.WriteLine("Model No: " + theATA.ModelNo);
                    console.WriteLine((String)"Block Size: " + theATA.BlockSize + " bytes");
                    console.WriteLine((String)"Block Count: " + theATA.Blocks);
                    console.WriteLine((String)"Size: " + ((theATA.Blocks*theATA.BlockSize) >> 20) + " MB");
                    console.WriteLine((String)"Max Write Pio Blocks: " + theATA.MaxWritePioBlocks);

                    numDrives++;
                }
                else if (aDevice is PATAPI)
                {
                    console.WriteLine();
                    console.Write("--------------------- Device ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");
                    console.WriteLine("Type: PATAPI");
                    console.WriteLine("Warning: Read-only support.");

                    PATAPI theATA = (PATAPI)aDevice;
                    console.WriteLine("Serial No: " + theATA.SerialNo);
                    console.WriteLine("Firmware Rev: " + theATA.FirmwareRev);
                    console.WriteLine("Model No: " + theATA.ModelNo);
                    console.WriteLine((String)"Block Size: " + theATA.BlockSize + " bytes");
                    console.WriteLine((String)"Block Count: " + theATA.Blocks);
                    console.WriteLine((String)"Size: " + ((theATA.Blocks*theATA.BlockSize) >> 20) + " MB");
                    console.WriteLine((String)"Max Write Pio Blocks: " + theATA.MaxWritePioBlocks);

                    numDrives++;
                }
                else if (aDevice is SATA)
                {
                    console.WriteLine();
                    console.Write("--------------------- Device ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");
                    console.WriteLine("Type: SATA");
                    console.WriteLine("Warning: This disk device type is not supported.");
                }
                else if (aDevice is SATAPI)
                {
                    console.WriteLine();
                    console.Write("--------------------- Device ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");
                    console.WriteLine("Type: SATAPI");
                    console.WriteLine("Warning: This disk device type is not supported.");
                }
            }

            console.Write("Total # of drives: ");
            console.WriteLine_AsDecimal(numDrives);
        }

        public static int IRQHandler(uint irqNumber)
        {
            PATAPI.IRQHandler(irqNumber);

            return 0;
        }
    }
}