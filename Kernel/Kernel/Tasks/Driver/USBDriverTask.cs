using Kernel.FOS_System;
using Kernel.FOS_System.Processes;
using Kernel.Hardware;
using Kernel.Hardware.USB;

namespace Kernel.Tasks.Driver
{
    public static class USBDriverTask
    {
        private static Consoles.VirtualConsole console;

        private static uint GCThreadId;

        public static void Main()
        {
            Helpers.ProcessInit("USB Driver", out GCThreadId);
            
            try
            {
                BasicConsole.WriteLine("USB Driver > Registering for IRQs...");
                SystemCalls.RegisterIRQHandler(-1, IRQHandler);

                BasicConsole.WriteLine("USB Driver > Creating virtual console...");
                console = new Consoles.VirtualConsole();

                BasicConsole.WriteLine("USB Driver > Connecting virtual console...");
                console.Connect();
                
                BasicConsole.WriteLine("USB Driver > Executing.");

                try
                {
                    BasicConsole.WriteLine("USB Driver > Initialising Device Manager...");
                    DeviceManager.Init();

                    BasicConsole.WriteLine("USB Driver > Initialising USB Manager...");
                    USBManager.Init();

                    BasicConsole.WriteLine("USB Driver > Outputing USB info...");
                    OutputUSB();
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
        
        /// <summary>
        /// Outputs the USB system information.
        /// </summary>
        private static void OutputUSB()
        {
            console.WriteLine(((FOS_System.String)"USB system initialised.        HCIs : ") + Hardware.USB.USBManager.HCIDevices.Count);
            console.WriteLine(((FOS_System.String)"                              UHCIs : ") + Hardware.USB.USBManager.NumUHCIDevices);
            console.WriteLine(((FOS_System.String)"                              OHCIs : ") + Hardware.USB.USBManager.NumOHCIDevices);
            console.WriteLine(((FOS_System.String)"                              EHCIs : ") + Hardware.USB.USBManager.NumEHCIDevices);
            console.WriteLine(((FOS_System.String)"                              xHCIs : ") + Hardware.USB.USBManager.NumxHCIDevices);
            console.WriteLine(((FOS_System.String)"                        USB devices : ") + Hardware.USB.USBManager.Devices.Count);

            int numDrives = 0;
            for (int i = 0; i < DeviceManager.Devices.Count; i++)
            {
                Device aDevice = (Device)DeviceManager.Devices[i];

                if (aDevice is Hardware.USB.HCIs.HCI)
                {
                    Hardware.USB.HCIs.HCI hciDevice = (Hardware.USB.HCIs.HCI)aDevice;
                    console.WriteLine();

                    console.Write("--------------------- HCI ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");

                    FOS_System.String statusText = "";
                    switch (hciDevice.Status)
                    {
                        case Hardware.USB.HCIs.HCI.HCIStatus.Dead:
                            statusText = "Dead";
                            break;
                        case Hardware.USB.HCIs.HCI.HCIStatus.Unset:
                            statusText = "Unset";
                            break;
                        case Hardware.USB.HCIs.HCI.HCIStatus.Active:
                            statusText = "Active";
                            break;
                        default:
                            statusText = "Uncreognised (was a new status type added?)";
                            break;
                    }
                    console.WriteLine("Status: " + statusText);
                }
                else if (aDevice is Hardware.USB.Devices.USBDevice)
                {
                    Hardware.USB.Devices.USBDevice usbDevice = (Hardware.USB.Devices.USBDevice)aDevice;
                    Hardware.USB.Devices.USBDeviceInfo usbDeviceInfo = usbDevice.DeviceInfo;
                    console.WriteLine();

                    console.Write("--------------------- Device ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");

                    if (aDevice is Hardware.USB.Devices.MassStorageDevice)
                    {
                        console.WriteLine("USB Mass Storage Device found.");
                        Hardware.USB.Devices.MassStorageDevice theMSD = (Hardware.USB.Devices.MassStorageDevice)usbDevice;
                        Hardware.USB.Devices.MassStorageDevice_DiskDevice theMSDDisk = theMSD.diskDevice;

                        console.Write("Disk device num: ");
                        console.WriteLine_AsDecimal(DeviceManager.Devices.IndexOf(theMSDDisk));
                        console.WriteLine(((FOS_System.String)"Block Size: ") + theMSDDisk.BlockSize + " bytes");
                        console.WriteLine(((FOS_System.String)"Block Count: ") + theMSDDisk.BlockCount);
                        console.WriteLine(((FOS_System.String)"Size: ") + ((theMSDDisk.BlockCount * theMSDDisk.BlockSize) >> 20) + " MB");

                        numDrives++;
                    }
                    else
                    {
                        console.WriteLine("Unrecognised USB device found.");
                    }

                    console.WriteLine();

                    if (usbDeviceInfo.usbSpec == 0x0100 || usbDeviceInfo.usbSpec == 0x0110 || usbDeviceInfo.usbSpec == 0x0200 || usbDeviceInfo.usbSpec == 0x0201 || usbDeviceInfo.usbSpec == 0x0210 || usbDeviceInfo.usbSpec == 0x0213 || usbDeviceInfo.usbSpec == 0x0300)
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
                    console.WriteLine(usbDeviceInfo.ManufacturerString.Value);
                    console.Write("Product:           ");
                    console.WriteLine(usbDeviceInfo.ProductString.Value);
                    console.Write("Serial number:     ");
                    console.WriteLine(usbDeviceInfo.SerialNumberString.Value);
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
