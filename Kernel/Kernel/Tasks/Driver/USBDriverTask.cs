using Kernel.Consoles;
using Kernel.Devices;
using Kernel.Framework;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes;
using Kernel.PCI;
using Kernel.USB;
using Kernel.USB.Devices;
using Kernel.USB.HCIs;

namespace Kernel.Tasks.Driver
{
    public static class USBDriverTask
    {
        private static VirtualConsole console;

        private static uint GCThreadId;
        private static uint USBUpdateThreadId;

        public static bool Terminating = false;

        public static void Main()
        {
            Helpers.ProcessInit("USB Driver", out GCThreadId);

            try
            {
                BasicConsole.WriteLine("USB Driver > Registering for IRQs...");
                SystemCalls.RegisterIRQHandler(-1, IRQHandler);

                BasicConsole.WriteLine("USB Driver > Creating virtual console...");
                console = new VirtualConsole();

                BasicConsole.WriteLine("USB Driver > Connecting virtual console...");
                console.Connect();

                BasicConsole.WriteLine("USB Driver > Executing.");

                DeviceManager.InitForProcess();

                try
                {
                    BasicConsole.WriteLine("USB Driver > Initialising USB Manager...");
                    USBManager.Init();

                    BasicConsole.WriteLine("USB Driver > Starting update thread...");
                    SystemCallResults SysCallResult = SystemCalls.StartThread(USBUpdateThreadMain, out USBUpdateThreadId);
                    if (SysCallResult != SystemCallResults.OK)
                    {
                        BasicConsole.WriteLine("USB Driver > Failed to create update thread!");
                    }

                    while (!Terminating)
                    {
                        try
                        {
                            BasicConsole.WriteLine("USB Driver > Initialising USB HCIs...");
                            USBManager.InitHCIs();
                        }
                        catch
                        {
                            if (!(ExceptionMethods.CurrentException is NotSupportedException))
                            {
                                BasicConsole.WriteLine("USB Driver > Error initialising HCIs!");
                                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                            }
                        }

                        BasicConsole.WriteLine("USB Driver > Outputting USB info...");
                        OutputUSB();

                        SystemCalls.SleepThread(10000);
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("USB Driver > Error executing!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }

                BasicConsole.WriteLine("USB Driver > Execution complete.");
            }
            catch
            {
                BasicConsole.WriteLine("USB Driver > Error initialising!");
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            }

            BasicConsole.WriteLine("USB Driver > Exiting...");
        }

        private static void USBUpdateThreadMain()
        {
            while (!Terminating)
            {
                if (!USBManager.UpdateRequired)
                {
                    if (SystemCalls.WaitSemaphore(USBManager.UpdateSemaphoreId) != SystemCallResults.OK)
                    {
                        BasicConsole.WriteLine("USB Driver > Failed to wait on update semaphore!");
                        SystemCalls.SleepThread(5000);
                    }
                }

                USBManager.Update();
            }
        }

        /// <summary>
        ///     Outputs the USB system information.
        /// </summary>
        private static void OutputUSB()
        {
            UnicodeString NullUnicodeString = new UnicodeString()
            {
                StringType = 0x00,
                Value = "[NULL]"
            };

            console.WriteLine((String)"USB system initialised.        HCIs : " + USBManager.HCIDevices.Count);
            console.WriteLine((String)"                              UHCIs : " + USBManager.NumUHCIDevices);
            console.WriteLine((String)"                              OHCIs : " + USBManager.NumOHCIDevices);
            console.WriteLine((String)"                              EHCIs : " + USBManager.NumEHCIDevices);
            console.WriteLine((String)"                              xHCIs : " + USBManager.NumxHCIDevices);
            console.WriteLine((String)"                        USB devices : " + USBManager.Devices.Count);

            int numDrives = 0;
            for (int i = 0; i < USBManager.Devices.Count; i++)
            {
                Device aDevice = (Device)USBManager.Devices[i];

                if (aDevice is HCI)
                {
                    HCI hciDevice = (HCI)aDevice;
                    console.WriteLine();

                    console.Write("--------------------- HCI ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");

                    String statusText = "";
                    switch (hciDevice.Status)
                    {
                        case HCI.HCIStatus.Dead:
                            statusText = "Dead";
                            break;
                        case HCI.HCIStatus.Unset:
                            statusText = "Unset";
                            break;
                        case HCI.HCIStatus.Active:
                            statusText = "Active";
                            break;
                        default:
                            statusText = "Uncreognised (was a new status type added?)";
                            break;
                    }
                    console.WriteLine("Status: " + statusText);
                }
                else if (aDevice is USBDevice)
                {
                    USBDevice usbDevice = (USBDevice)aDevice;
                    USBDeviceInfo usbDeviceInfo = usbDevice.DeviceInfo;
                    console.WriteLine();

                    console.Write("--------------------- Device ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");

                    if (aDevice is MassStorageDevice)
                    {
                        console.WriteLine("USB Mass Storage Device found.");
                        MassStorageDevice theMSD = (MassStorageDevice)usbDevice;
                        MassStorageDevice_DiskDevice theMSDDisk = theMSD.diskDevice;

                        console.Write("Disk device num: ");
                        console.WriteLine_AsDecimal(USBManager.Devices.IndexOf(theMSDDisk));
                        console.WriteLine((String)"Block Size: " + theMSDDisk.BlockSize + " bytes");
                        console.WriteLine((String)"Block Count: " + theMSDDisk.Blocks);
                        console.WriteLine((String)"Size: " + ((theMSDDisk.Blocks*theMSDDisk.BlockSize) >> 20) +
                                          " MB");

                        numDrives++;
                    }
                    else
                    {
                        console.WriteLine("Unrecognised USB device found.");
                    }

                    console.WriteLine();

                    if (usbDeviceInfo.usbSpec == 0x0100 || usbDeviceInfo.usbSpec == 0x0110 ||
                        usbDeviceInfo.usbSpec == 0x0200 || usbDeviceInfo.usbSpec == 0x0201 ||
                        usbDeviceInfo.usbSpec == 0x0210 || usbDeviceInfo.usbSpec == 0x0213 ||
                        usbDeviceInfo.usbSpec == 0x0300)
                    {
                        console.Write("USB ");
                        console.Write_AsDecimal((usbDeviceInfo.usbSpec >> 8) & 0xFF);
                        console.Write(".");
                        console.WriteLine_AsDecimal(usbDeviceInfo.usbSpec & 0xFF);
                    }
                    else
                    {
                        console.ErrorColour();
                        console.Write("Invalid USB version ");
                        console.Write_AsDecimal((usbDeviceInfo.usbSpec >> 8) & 0xFF);
                        console.Write(".");
                        console.WriteLine_AsDecimal(usbDeviceInfo.usbSpec & 0xFF);
                        console.DefaultColour();
                    }

                    if (usbDeviceInfo.usbClass == 0x09)
                    {
                        switch (usbDeviceInfo.usbProtocol)
                        {
                            case 0:
                                console.WriteLine(" - Full speed USB hub");
                                break;
                            case 1:
                                console.WriteLine(" - Hi-speed USB hub with single TT");
                                break;
                            case 2:
                                console.WriteLine(" - Hi-speed USB hub with multiple TTs");
                                break;
                        }
                    }

                    console.Write("Endpoint 0 mps: ");
                    console.Write_AsDecimal(((Endpoint)usbDeviceInfo.Endpoints[0]).MPS);
                    console.WriteLine(" byte.");
                    console.Write("Vendor:            ");
                    console.WriteLine(usbDeviceInfo.vendor);
                    console.Write("Product:           ");
                    console.WriteLine(usbDeviceInfo.product);
                    console.Write("Release number:    ");
                    console.Write_AsDecimal((usbDeviceInfo.releaseNumber >> 8) & 0xFF);
                    console.Write(".");
                    console.WriteLine_AsDecimal(usbDeviceInfo.releaseNumber & 0xFF);
                    console.Write("Manufacturer:      ");
                    console.Write((usbDeviceInfo.ManufacturerString ?? NullUnicodeString).Value);
                    console.WriteLine();
                    console.Write("Product:           ");
                    console.Write((usbDeviceInfo.ProductString ?? NullUnicodeString).Value);
                    console.WriteLine();
                    console.Write("Serial number:     ");
                    console.Write((usbDeviceInfo.SerialNumberString ?? NullUnicodeString).Value);
                    console.WriteLine();
                    console.Write("Number of config.: ");
                    console.WriteLine_AsDecimal(usbDeviceInfo.numConfigurations); // number of possible configurations
                    console.Write("MSDInterfaceNum:   ");
                    console.WriteLine_AsDecimal(usbDeviceInfo.MSD_InterfaceNum);
                    SystemCalls.SleepThread(1000);
                }
            }
        }

        public static int IRQHandler(uint irqNumber)
        {
            USBManager.IRQHandler();

            return 0;
        }
    }
}