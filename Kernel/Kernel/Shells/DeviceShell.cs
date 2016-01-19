using Kernel.Hardware;
using Kernel.Hardware.ATA;
using Kernel.Hardware.Devices;
using Kernel.Hardware.PCI;
using Kernel.Hardware.USB;
using Kernel.Hardware.IO.Serial;

namespace Kernel.Shells
{
    public class DeviceShell : Shell
    {
        public DeviceShell()
            : base()
        {
        }
        public DeviceShell(Console AConsole, Keyboard AKeyboard)
            : base(AConsole, AKeyboard)
        {
        }
        
        public override void Execute()
        {
            try
            {
                DeviceManager.Init();

                //TODO: These need transferring from KernelTask memory (somehow) before we can add them to device manager
                //      Really they shouldn't be initialised by Kernel Task nor used by Kernel/Debugger Tasks directly.
                //      But they are needed for from-startup/first-instance/fail-proof debugging.
                //DeviceManager.AddDevice(Serial.COM1);
                //DeviceManager.AddDevice(Serial.COM2);
                //DeviceManager.AddDevice(Serial.COM3);

                BasicConsole.WriteLine("DM > Initialising ATA Manager...");
                ATAManager.Init();
                BasicConsole.WriteLine("DM > Initialising PCI Manager...");
                PCIManager.Init();
                BasicConsole.WriteLine("DM > Initialising USB Manager...");
                USBManager.Init();
                BasicConsole.WriteLine("DM > Initialisation completed.");

                while (!terminating)
                {
                    try
                    {
                        KeyboardKey k = keyboard.ReadKey();
                        switch (k)
                        {
                            case KeyboardKey.A:
                                OutputATA();
                                break;  
                            case KeyboardKey.P:
                                OutputPCI();
                                break;
                            case KeyboardKey.U:
                                OutputUSB();
                                break;
                            default:
                                console.WriteLine("Unrecognised. Options are (A):ATA, (P):PCI, (U):USB");
                                break;
                        }
                    }
                    catch
                    {
                        OutputExceptionInfo(ExceptionMethods.CurrentException);
                    }
                }
            }
            catch
            {
                OutputExceptionInfo(ExceptionMethods.CurrentException);
                //Pause to give us the chance to read the output. 
                //  We do not know what the code outside this shell may do.
                Processes.SystemCalls.SleepThread(1000);
            }
            console.WriteLine("Device shell exited.");
        }

        /// <summary>
        /// Outputs the USB system information.
        /// </summary>
        private void OutputUSB()
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
                    Processes.SystemCalls.SleepThread(1000);
                }
            }
        }
        /// <summary>
        /// Outputs the ATA system information.
        /// </summary>
        private void OutputATA()
        {
            int numDrives = 0;
            for (int i = 0; i < DeviceManager.Devices.Count; i++)
            {
                Device aDevice = (Device)DeviceManager.Devices[i];
                if (aDevice is PATA)
                {
                    console.WriteLine();
                    console.Write("--------------------- Device ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");
                    console.WriteLine("Type: PATA");

                    PATA theATA = (PATA)aDevice;
                    console.WriteLine(((FOS_System.String)"Serial No: ") + theATA.SerialNo);
                    console.WriteLine(((FOS_System.String)"Firmware Rev: ") + theATA.FirmwareRev);
                    console.WriteLine(((FOS_System.String)"Model No: ") + theATA.ModelNo);
                    console.WriteLine(((FOS_System.String)"Block Size: ") + theATA.BlockSize + " bytes");
                    console.WriteLine(((FOS_System.String)"Block Count: ") + theATA.BlockCount);
                    console.WriteLine(((FOS_System.String)"Size: ") + ((theATA.BlockCount * theATA.BlockSize) >> 20) + " MB");
                    console.WriteLine(((FOS_System.String)"Max Write Pio Blocks: ") + (theATA.MaxWritePioBlocks));

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
                    console.WriteLine(((FOS_System.String)"Serial No: ") + theATA.SerialNo);
                    console.WriteLine(((FOS_System.String)"Firmware Rev: ") + theATA.FirmwareRev);
                    console.WriteLine(((FOS_System.String)"Model No: ") + theATA.ModelNo);
                    console.WriteLine(((FOS_System.String)"Block Size: ") + theATA.BlockSize + " bytes");
                    console.WriteLine(((FOS_System.String)"Block Count: ") + theATA.BlockCount);
                    console.WriteLine(((FOS_System.String)"Size: ") + ((theATA.BlockCount * theATA.BlockSize) >> 20) + " MB");
                    console.WriteLine(((FOS_System.String)"Max Write Pio Blocks: ") + (theATA.MaxWritePioBlocks));
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
        /// <summary>
        /// Outputs the PCI system information.
        /// </summary>
        private void OutputPCI()
        {
            for (int i = 0; i < PCIManager.Devices.Count; i++)
            {
                PCIDevice aDevice = (PCIDevice)PCIManager.Devices[i];
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
