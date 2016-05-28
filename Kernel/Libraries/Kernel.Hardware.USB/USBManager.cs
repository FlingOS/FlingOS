#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

//#define USB_TRACE

using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.Processes;
using Kernel.FOS_System.Processes.Requests.Devices;
using Kernel.Hardware;
using Kernel.Hardware.Devices;
using Kernel.Hardware.PCI;
using Kernel.Hardware.Processes;
using Kernel.USB.Devices;
using Kernel.USB.HCIs;

namespace Kernel.USB
{
    //TODO: Read Benjamin Lunt's book "USB: The Universal Serial Bus (FYSOS: Operating System Design Book 8)"
    //  It contains a lot of practical points about USB implementation which this driver fails to account for.
    //  For example, this driver does not follow the practice of requesting the first 8 bytes of descriptors,
    //  to get the length info, then requesting the remaining bytes. Nor does it separately send the STATUS
    //  packet at the end of a successful request. 
    //
    //  For this driver to be 100% proper, it should be modified to include Lunt's practical notes. However,
    //  this is only the low-level USB driver which will be superseded by the proper USB driver in the full 
    //  Driver Framework, so for now, this will do.

    /// <summary>
    ///     Provides methods for managing USB access.
    /// </summary>
    public static unsafe class USBManager
    {
        public static bool IgnoreUSB10and11Devices;

        public static bool UpdateRequired;
        public static int UpdateSemaphoreId;

        /// <summary>
        ///     The number of UHCI devices detected.
        /// </summary>
        public static int NumUHCIDevices { get; private set; }

        /// <summary>
        ///     The number of OHCI devices detected.
        /// </summary>
        public static int NumOHCIDevices { get; private set; }

        /// <summary>
        ///     The number of EHCI devices detected.
        /// </summary>
        public static int NumEHCIDevices { get; private set; }

        /// <summary>
        ///     The number of xHCI devices detected.
        /// </summary>
        public static int NumxHCIDevices { get; private set; }

        /// <summary>
        ///     List of all the HCI device instances.
        /// </summary>
        public static List HCIDevices;

        /// <summary>
        ///     List of all the USB device instances.
        /// </summary>
        public static List Devices;

        public static void Init()
        {
            IgnoreUSB10and11Devices = true;
            UpdateRequired = false;
            HCIDevices = new List(3);
            Devices = new List(5);

            if (SystemCalls.CreateSemaphore(0, out UpdateSemaphoreId) != SystemCallResults.OK)
            {
                ExceptionMethods.Throw(new Exception("Couldn't allocate semaphore for USB Manager!"));
            }
        }

        /// <summary>
        ///     Initialises USB management. Scans the PCI bus for HCIs and initialises any supported HCIs that are found.
        /// </summary>
        public static void InitHCIs()
        {
            //Enumerate PCI devices looking for (unclaimed) USB host controllers

            //      UHCI:  Class ID: 0x0C, Sub-class: 0x03, Prog(ramming) Interface: 0x00
            //      OHCI:  Class ID: 0x0C, Sub-class: 0x03, Prog(ramming) Interface: 0x10
            //      EHCI:  Class ID: 0x0C, Sub-class: 0x03, Prog(ramming) Interface: 0x20
            //      xHCI:  Class ID: 0x0C, Sub-class: 0x03, Prog(ramming) Interface: 0x30

            if (IgnoreUSB10and11Devices)
            {
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(
                    "USB driver will ignore USB 1.0 and 1.1 mode devices (Low and full-speed devices).");
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            List AllDevices = DeviceManager.GetDeviceList();
            List HCIPCIDevices = new List();
            for (int i = 0; i < AllDevices.Count; i++)
            {
                Device aDevice = (Device) AllDevices[i];
                if (!aDevice.Claimed)
                {
                    if (aDevice.Class == DeviceClass.Generic && aDevice.SubClass == DeviceSubClass.PCI)
                    {
                        if (DeviceManager.ClaimDevice(aDevice))
                        {
                            bool release = true;

                            if (DeviceManager.FillDeviceInfo(aDevice))
                            {
                                PCIDevice pciDevice = new PCIDevice(aDevice.Info[0], aDevice.Info[1], aDevice.Info[2],
                                    "Generic PCI Device");
                                if (pciDevice.HeaderType == PCIDevice.PCIHeaderType.Normal)
                                {
                                    pciDevice = new PCIDeviceNormal(aDevice.Info[0], aDevice.Info[1], aDevice.Info[2]);
                                    //0x0C = Serial bus controllers
                                    if (pciDevice.ClassCode == 0x0C)
                                    {
                                        //0x03 = USB controllers
                                        if (pciDevice.Subclass == 0x03)
                                        {
                                            HCIPCIDevices.Add(pciDevice);
                                            release = false;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                BasicConsole.WriteLine("Error! USBManager couldn't fill PCI device info!");
                            }

                            if (release)
                            {
                                if (!DeviceManager.ReleaseDevice(aDevice))
                                {
                                    BasicConsole.WriteLine("Error! USBManager couldn't release PCI device!");
                                }
                            }
                        }
                        else
                        {
                            BasicConsole.WriteLine("Error! USBManager couldn't claim PCI device!");
                        }
                    }
                }
            }

            for (int i = 0; i < HCIPCIDevices.Count; i++)
            {
                PCIDevice aDevice = (PCIDevice) HCIPCIDevices[i];
                //xHCI = 0x30
                if (aDevice.ProgIF == 0x30)
                {
                    //xHCI detected
#if USB_TRACE
                    BasicConsole.WriteLine("xHCI detected.");
#endif

                    //TODO: Add xHCI support
                    //Supported by VMWare
                    //  - This is USB 3.0

                    NumxHCIDevices++;
                }
            }
            for (int i = 0; i < HCIPCIDevices.Count; i++)
            {
                PCIDevice aDevice = (PCIDevice) HCIPCIDevices[i];
                //EHCI = 0x20
                if (aDevice.ProgIF == 0x20)
                {
                    //EHCI detected
#if USB_TRACE
                    BasicConsole.WriteLine("EHCI detected.");
#endif
                    NumEHCIDevices++;

                    PCIDeviceNormal EHCI_PCIDevice = (PCIDeviceNormal) aDevice;
                    EHCI_PCIDevice.Claimed = true;

                    //BasicConsole.SetTextColour(BasicConsole.warning_colour);
                    //BasicConsole.WriteLine("WARNING! EHCI device support disabled.");
                    //BasicConsole.SetTextColour(BasicConsole.default_colour);
                    EHCI newEHCI = new EHCI(EHCI_PCIDevice);
                    HCIDevices.Add(newEHCI);

                    DeviceManager.RegisterDevice(newEHCI);
                    newEHCI.Start();
                }
            }
            for (int i = 0; i < HCIPCIDevices.Count; i++)
            {
                PCIDevice aDevice = (PCIDevice) HCIPCIDevices[i];
                //UHCI = 0x00
                if (aDevice.ProgIF == 0x00)
                {
                    //UHCI detected
#if USB_TRACE
                    BasicConsole.WriteLine("UHCI detected.");
#endif
                    NumUHCIDevices++;

                    PCIDeviceNormal UHCI_PCIDevice = (PCIDeviceNormal) aDevice;
                    UHCI_PCIDevice.Claimed = true;

                    UHCI newUHCI = new UHCI(UHCI_PCIDevice);
                    HCIDevices.Add(newUHCI);

                    DeviceManager.RegisterDevice(newUHCI);
                    newUHCI.Start();
                }
                //OHCI = 0x10
                else if (aDevice.ProgIF == 0x10)
                {
                    //OHCI detected
#if USB_TRACE
                    BasicConsole.WriteLine("OHCI detected.");
#endif

                    //TODO: Add OHCI support
                    //Not supported by VMWare or my laptop 
                    //  so we aren't going to program this any further for now.

                    NumOHCIDevices++;
                }
            }
        }

        public static void NotifyDevicesNeedUpdate()
        {
            UpdateRequired = true;
            if (!ProcessManager.Semaphore_Signal(UpdateSemaphoreId, ProcessManager.CurrentProcess))
            {
                BasicConsole.WriteLine("USB Manager couldn't signal update semaphore!");
                //ExceptionMethods.Throw(new FOS_System.Exception("USB Manager couldn't signal update semaphore!"));
            }
        }

        /// <summary>
        ///     Updates the USB manager and all host controller devices.
        /// </summary>
        public static void Update()
        {
            UpdateRequired = false;
            for (int i = 0; i < HCIDevices.Count; i++)
            {
                ((HCI) HCIDevices[i]).Update();
            }
        }

        /// <summary>
        ///     Creates new device info for the specified port.
        /// </summary>
        /// <param name="hc">The host contorller which owns the port.</param>
        /// <param name="port">The port to create device info for.</param>
        /// <returns>The new device info.</returns>
        public static USBDeviceInfo CreateDeviceInfo(HCI hc, HCPort port)
        {
#if USB_TRACE
            DBGMSG("Creating USB device...");
#endif
            USBDeviceInfo deviceInf = new USBDeviceInfo(port.portNum, hc);
            deviceInf.Configurations = new List(1);
            deviceInf.Interfaces = new List(1);
            deviceInf.Endpoints = new List(1);
            deviceInf.Endpoints.Add(new Endpoint());
            ((Endpoint) deviceInf.Endpoints[0]).MPS = 64;
            ((Endpoint) deviceInf.Endpoints[0]).Type = Endpoint.Types.BIDIR;
            ((Endpoint) deviceInf.Endpoints[0]).Toggle = false;
#if USB_TRACE
            DBGMSG("Created device.");
#endif
            return deviceInf;
        }

        /// <summary>
        ///     Creates and initialises a new USb device for the specified device info and address.
        /// </summary>
        /// <param name="deviceInfo">The device info to create a new device for.</param>
        /// <param name="address">The USB address for the new device.</param>
        public static void SetupDevice(USBDeviceInfo deviceInfo, byte address)
        {
            deviceInfo.address = 0; // device number has to be set to 0
            bool success = false;

            try
            {
                // Windows Legacy Compatiblity code
                //  See:
                //      USB: The Universal Serial Bus (FYSOS: Operating System Design Book 8)
                //      Part 2 : Chapter 11 : Device Enumeration with the UHCI
                //          -> Inserting your queue into the stack, Paragraph 6
                //  "
                //      The main reason is that some devices were only expected to work with the Windows
                //      Operating System. Win98SE for example, gets the first 8 bytes, does a reset of the port,
                //      sets the address of the device and then finally gets all 18 bytes of the descriptor.
                //      Therefore, these few devices don't expect to send more than the first 8 bytes while in the
                //      default state, the state before it is given an address, and may not function properly until
                //      that sequence is received. It completely defies the given operation of the USB
                //      specification, but this is how some devices function. I have not personally seen or used 
                //      any device that functions in this way, but have read documentation that states this may 
                //      happen.
                //  "

                HCPort port = deviceInfo.hc.GetPort(deviceInfo.portNum);
                USBPortSpeed speed = port.speed;
                if (speed == USBPortSpeed.Low ||
                    speed == USBPortSpeed.Full)
                {
                    if (IgnoreUSB10and11Devices)
                    {
#if USB_TRACE
                        BasicConsole.SetTextColour(BasicConsole.warning_colour);
                        BasicConsole.WriteLine("Ignoring USB 1.0 or 1.1 device!");
                        BasicConsole.SetTextColour(BasicConsole.default_colour);
#endif
                        return;
                    }

                    success = GetDeviceDescriptor(deviceInfo, true);
                    if (!success)
                    {
                        success = GetDeviceDescriptor(deviceInfo, true);
                    }

                    if (!success)
                    {
#if USB_TRACE
                        DBGMSG("Partial device descriptor could not be read! Setup device aborted.");
                        BasicConsole.DelayOutput(10);
#endif
                        return;
                    }

#if USB_TRACE
                    BasicConsole.DelayOutput(3);
#endif

                    port.Reset();

                    deviceInfo.address = SetDeviceAddress(deviceInfo, address);

                    success = GetDeviceDescriptor(deviceInfo, false);
                    if (!success)
                    {
                        success = GetDeviceDescriptor(deviceInfo, false);
                    }

                    if (!success)
                    {
#if USB_TRACE
                        DBGMSG("Full device descriptor could not be read! Setup device aborted.");
                        BasicConsole.DelayOutput(10);
#endif
                        return;
                    }
                }
                else
                {
                    success = GetDeviceDescriptor(deviceInfo);
                    if (!success)
                    {
                        success = GetDeviceDescriptor(deviceInfo);
                    }

                    if (!success)
                    {
#if USB_TRACE
                        DBGMSG("Device descriptor could not be read! Setup device aborted.");
                        BasicConsole.DelayOutput(10);
#endif
                        return;
                    }

#if USB_TRACE
                    BasicConsole.DelayOutput(3);
#endif

                    deviceInfo.address = SetDeviceAddress(deviceInfo, address);
                }

                bool hub = deviceInfo.usbClass == 0x09;
#if USB_TRACE
                if (hub)
                {
                    DBGMSG(" <-- usb Hub");
                    BasicConsole.DelayOutput(2);
                }
#endif

                success = GetConfigurationDescriptors(deviceInfo);
                if (!success)
                {
                    success = GetConfigurationDescriptors(deviceInfo);
                }

                if (!success)
                {
#if USB_TRACE
                    DBGMSG("Config descriptor could not be read! Setup device aborted.");
                    BasicConsole.DelayOutput(10);
#endif
                    return;
                }

#if USB_TRACE
                DBGMSG("Got config descriptor.");
                BasicConsole.DelayOutput(4);
#endif

                byte wantedConfig = 1;
                SetConfiguration(deviceInfo, wantedConfig); // set first configuration

#if USB_TRACE
    // Debug check: Check configuration set properly
                byte config = GetConfiguration(deviceInfo);
                if (config == wantedConfig)
                {
                    DBGMSG("Configuration OK");
                }
                else
                {
                    DBGMSG(((FOS_System.String)"Configuration not OK! wantedConfig=") + wantedConfig + ", config=" + config);
                }
                BasicConsole.DelayOutput(10);
#endif
                // Hub device
                if (hub)
                {
//TODO: Uncomment these #if/#endif when hub driver is done
//#if USB_TRACE
                    BasicConsole.WriteLine("-------------------------- Hub --------------------------");
                    BasicConsole.DelayOutput(2);
                    //#endif

                    //TODO: Hub driver

                    //For now, create a completely generic device instance so we don't lose track of
                    //  this device entirely.
                    USBDevice NewDevice = new USBDevice(deviceInfo, DeviceGroup.USB, DeviceClass.Generic,
                        DeviceSubClass.USB, "USB Hub", true);
                    DeviceManager.RegisterDevice(NewDevice);
                    Devices.Add(NewDevice);
                }
                // Mass Storage Device
                else if (deviceInfo.InterfaceClass == 0x08)
                {
#if USB_TRACE
                    DBGMSG("------------------ Mass Storage Device ------------------");
                    BasicConsole.DelayOutput(2);
#endif
                    try
                    {
                        MassStorageDevice NewDevice = new MassStorageDevice(deviceInfo);
                        DeviceManager.RegisterDevice(NewDevice);
                        Devices.Add(NewDevice);
                    }
                    catch
                    {
#if USB_TRACE
                        DBGMSG("Error creating USB device! Aborted creating device.");
#endif
                    }
                }
                // Unrecognised device
                else
                {
#if USB_TRACE
                    DBGMSG("----------- Unrecognised USB device detected. -----------");
                    BasicConsole.DelayOutput(2);
#endif
                    //For now, create a completely generic device instance so we don't lose track of
                    //  this device entirely.
                    USBDevice NewDevice = new USBDevice(deviceInfo, DeviceGroup.USB, DeviceClass.Generic,
                        DeviceSubClass.USB, "Unknown USB Device", true);
                    DeviceManager.RegisterDevice(NewDevice);
                    Devices.Add(NewDevice);
                }
            }
            catch
            {
                //No need to do any device cleanup because we only gathered device info. We didn't create
                //  the USBDevice unless we were 100% successful :)

                //Need to do port cleanup
                deviceInfo.FreePort();
#if USB_TRACE
                DBGMSG("Error caught while setting up USB device! Aborted setup.");
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
#endif
            }
        }

        /// <summary>
        ///     Sets the USB device address of the specified device to the specified value.
        /// </summary>
        /// <param name="device">The device to set the address of.</param>
        /// <param name="addr">The address to set to.</param>
        /// <returns>The address it was set to.</returns>
        public static byte SetDeviceAddress(USBDeviceInfo device, byte addr)
        {
#if USB_TRACE
            DBGMSG("");
            DBGMSG("USB: SET_ADDRESS");
#endif

            byte new_address = addr; // indicated port number

            USBTransfer transfer = new USBTransfer();
            device.hc.SetupTransfer(device, transfer, USBTransferType.Control, 0, 64);
            device.hc.SETUPTransaction(transfer, 8, 0x00, 5, 0, new_address, 0, 0);
            device.hc.INTransaction(transfer, true, null, 0);
            device.hc.IssueTransfer(transfer);

#if USB_TRACE
            DBGMSG(((FOS_System.String)" > New address: ") + new_address);
            BasicConsole.DelayOutput(4);
#endif
            return new_address;
        }

        /// <summary>
        ///     Gets the device descriptor from the specified device.
        /// </summary>
        /// <param name="device">The device info of the device to get the descriptor from.</param>
        /// <returns>True if USB transfer completed successfully. Otherwise, false.</returns>
        public static bool GetDeviceDescriptor(USBDeviceInfo device, bool first8BytesOnly = false)
        {
#if USB_TRACE
            DBGMSG("USB: GET_DESCRIPTOR Device");
#endif

            DeviceDescriptor* descriptor =
                (DeviceDescriptor*)
                    Heap.AllocZeroed((uint) sizeof(DeviceDescriptor), "USBManager : GetDeviceDescriptor");
            USBTransfer transfer = new USBTransfer();
            try
            {
                device.hc.SetupTransfer(device, transfer, USBTransferType.Control, 0, 64);
                device.hc.SETUPTransaction(transfer, 8, 0x80, 6, 1, 0, 0, first8BytesOnly ? (ushort) 8u : (ushort) 18u);
                device.hc.INTransaction(transfer, false, descriptor, first8BytesOnly ? (ushort) 8u : (ushort) 18u);
                device.hc.OUTTransaction(transfer, true, null, 0);
                device.hc.IssueTransfer(transfer);

                if (transfer.success)
                {
#if EHCI_TRACE
                    byte* bpDescriptor = (byte*)descriptor;
                    for (int i = 0; i < sizeof(DeviceDescriptor); i++)
                    {
                        DBGMSG(((FOS_System.String)"i=") + i + ", bpDescriptor[i]=" + bpDescriptor[i]);
                    }
#endif

                    ParseDeviceDescriptor(descriptor, device);
#if USB_TRACE
                    ShowDevice(device);
                    BasicConsole.DelayOutput(4);
#endif
                }
            }
            finally
            {
                Heap.Free(descriptor);
            }
            return transfer.success;
        }

        /// <summary>
        ///     Parses the specified device descriptor.
        /// </summary>
        /// <param name="d">The desxcriptor to parse.</param>
        /// <param name="usbDev">The device info for the device the descriptor came from.</param>
        private static void ParseDeviceDescriptor(DeviceDescriptor* d, USBDeviceInfo usbDev)
        {
            usbDev.usbSpec = d->bcdUSB;
            usbDev.usbClass = d->deviceClass;
            usbDev.usbSubclass = d->deviceSubclass;
            usbDev.usbProtocol = d->deviceProtocol;
            usbDev.vendor = d->VendorId;
            usbDev.product = d->ProductId;
            usbDev.releaseNumber = d->bcdDevice;
            usbDev.manufacturerStringID = d->manufacturer;
            usbDev.productStringID = d->product;
            usbDev.serialNumberStringID = d->serialNumber;
            usbDev.numConfigurations = d->numConfigurations;
            ((Endpoint) usbDev.Endpoints[0]).MPS = d->MaxPacketSize;

            usbDev.ManufacturerString = GetUnicodeStringDescriptor(usbDev, usbDev.manufacturerStringID);
            usbDev.ProductString = GetUnicodeStringDescriptor(usbDev, usbDev.productStringID);
            usbDev.SerialNumberString = GetUnicodeStringDescriptor(usbDev, usbDev.serialNumberStringID);
        }

        /// <summary>
        ///     Gets the configuration descriptor from the specified device.
        /// </summary>
        /// <param name="device">The device info of the device to get the descriptor from.</param>
        /// <returns>True if USB transfer completed successfully. Otherwise, false.</returns>
        public static bool GetConfigurationDescriptors(USBDeviceInfo device)
        {
#if USB_TRACE
            DBGMSG("USB: GET_DESCRIPTOR Config");
#endif

            //64 byte buffer
            ushort bufferSize = 64;
            byte* buffer = (byte*) Heap.AllocZeroed(bufferSize, "USBManager: GetConfigDescriptor");

            bool success = false;

            try
            {
                USBTransfer transfer = new USBTransfer();
                device.hc.SetupTransfer(device, transfer, USBTransferType.Control, 0, bufferSize);
                device.hc.SETUPTransaction(transfer, 8, 0x80, 6, 2, 0, 0, bufferSize);
                device.hc.INTransaction(transfer, false, buffer, 64);
                device.hc.OUTTransaction(transfer, true, null, 0);
                device.hc.IssueTransfer(transfer);

                success = transfer.success;

                if (transfer.success)
                {
                    byte currentConfig = GetConfiguration(device);

                    // parse to config (len=9,type=2), interface (len=9,type=4) or endpoint (len=7,type=5)
#if USB_TRACE
                    DBGMSG("---------------------------------------------------------------------");
#endif
                    byte* addr = buffer;
                    byte* lastByte = addr + bufferSize;

                    ushort numEndpoints = 1;
                    // First pass. Retrieve usb_interfaceDescriptor which contains the number of endpoints
                    while (addr < lastByte)
                    {
                        byte type = *(addr + 1);
                        byte length = *addr;

                        if (length == 9 && type == 2)
                        {
                            ConfigurationDescriptor* descriptor = (ConfigurationDescriptor*) addr;

                            Configuration config = new Configuration();
                            config.Attribs = (Configuration.Attributes) descriptor->attributes;
                            config.Selector = descriptor->configurationValue;
                            config.MaxPower = descriptor->maxPower;
                            config.NumInterfaces = descriptor->numInterfaces;
                            if (currentConfig == config.Selector)
                            {
                                config.Description = GetUnicodeStringDescriptor(device, descriptor->configuration);
                            }
                            else
                            {
                                config.Description = new UnicodeString
                                {
                                    StringType = 0,
                                    Value = "[Unable to load at this time]"
                                };
                            }

                            device.Configurations.Add(config);

#if USB_TRACE
                            ShowConfiguration(config);
#endif
                        }
                        else if (length == 9 && type == 4)
                        {
                            InterfaceDescriptor* descriptor = (InterfaceDescriptor*) addr;

                            Interface interf = new Interface();
                            interf.InterfaceNumber = descriptor->interfaceNumber;
                            interf.AlternateSetting = descriptor->alternateSetting;
                            interf.Class = descriptor->interfaceClass;
                            interf.Subclass = descriptor->interfaceSubclass;
                            interf.Protocol = descriptor->interfaceProtocol;
                            interf.Description = GetUnicodeStringDescriptor(device, descriptor->StringIndex);
                            interf.NumEndpoints = descriptor->numEndpoints;
                            device.Interfaces.Add(interf);

#if USB_TRACE
                            ShowInterface(interf);
#endif

                            if (interf.Class == 8)
                            {
                                // store interface number for mass storage transfers
                                device.MSD_InterfaceNum = interf.InterfaceNumber;
                                device.InterfaceClass = interf.Class;
                                device.InterfaceSubclass = interf.Subclass;
                            }

                            numEndpoints += interf.NumEndpoints;
                        }
                        else if (length == 7 && type == 5)
                        {
                            //Skip endpoints in first pass
                        }
                        else
                        {
#if USB_TRACE
                            DBGMSG(((FOS_System.String)"Unknown descriptor: Length=") + length + ", Type=" + type);
#endif
                            if (length == 0)
                            {
                                break;
                            }
                        }
                        addr += length;
                    }

                    Object endpointZero = device.Endpoints[0];
                    device.Endpoints.Empty();
                    device.Endpoints.Add(endpointZero);
                    for (int i = 0; i < numEndpoints - 1; i++)
                    {
                        device.Endpoints.Add(new Endpoint());
                    }

                    // Second pass. Fill in endpoint information
                    addr = buffer;
                    while (addr < lastByte)
                    {
                        byte type = *(addr + 1);
                        byte length = *addr;

                        if (length == 7 && type == 5)
                        {
                            EndpointDescriptor* descriptor = (EndpointDescriptor*) addr;

                            byte ep_id = (byte) (descriptor->endpointAddress & 0xF);
#if USB_TRACE
                            if (ep_id >= numEndpoints)
                            {
                                DBGMSG("ep_id >= numEndpoints!!");
                            }
#endif
                            Endpoint endpoint = (Endpoint) device.Endpoints[ep_id];

                            endpoint.MPS = descriptor->maxPacketSize;
                            endpoint.Type = Endpoint.Types.BIDIR; // Can be overwritten below
                            endpoint.Address = (byte) (descriptor->endpointAddress & 0xF);
                            endpoint.Attributes = descriptor->attributes;
                            endpoint.Interval = descriptor->interval;

                            // store endpoint numbers for IN/OUT mass storage transfers, attributes must be 0x2, because there are also endpoints with attributes 0x3(interrupt)
                            if ((descriptor->endpointAddress & 0x80) > 0 && descriptor->attributes == 0x2)
                            {
                                if (ep_id < 3)
                                {
                                    device.MSD_INEndpointID = ep_id;
                                }
                                endpoint.Type = Endpoint.Types.IN;
                            }

                            if ((descriptor->endpointAddress & 0x80) == 0 && descriptor->attributes == 0x2)
                            {
                                if (ep_id < 3)
                                {
                                    device.MSD_OUTEndpointID = ep_id;
                                }
                                endpoint.Type = Endpoint.Types.OUT;
                            }

#if USB_TRACE
                            ShowEndpoint(endpoint);
#endif
                        }
                        else if (length == 0)
                        {
                            break;
                        }

                        addr += length;
                    }
                }
            }
            finally
            {
                Heap.Free(buffer);
            }

            return success;
        }

        /// <summary>
        ///     Gets the device string descriptor from the specified device.
        /// </summary>
        /// <param name="device">The device info of the device to get the descriptor from.</param>
        /// <returns>True if USB transfer completed successfully. Otherwise, false.</returns>
        public static StringInfo GetDeviceStringDescriptor(USBDeviceInfo device)
        {
#if USB_TRACE
            DBGMSG("USB: GET_DESCRIPTOR string");
#endif

            StringInfo result = null;
            StringDescriptor* descriptor =
                (StringDescriptor*)
                    Heap.AllocZeroed((uint) sizeof(StringDescriptor), "USBManager : GetDeviceStringDescriptor");

            try
            {
                ushort size = (ushort) sizeof(StringDescriptor);
                USBTransfer transfer = new USBTransfer();
                device.hc.SetupTransfer(device, transfer, USBTransferType.Control, 0, 64);
                device.hc.SETUPTransaction(transfer, 8, 0x80, 6, 3, 0, 0, size);
                device.hc.INTransaction(transfer, false, descriptor, size);
                device.hc.OUTTransaction(transfer, true, null, 0);
                device.hc.IssueTransfer(transfer);

                if (transfer.success)
                {
                    if (descriptor->length > 0)
                    {
                        int totalLangs = 0;
                        for (int i = 0; i < 10; i++)
                        {
                            if (descriptor->languageID[i] >= 0x0400 && descriptor->languageID[i] <= 0x0465)
                            {
                                totalLangs++;
                            }
                        }

                        result = new StringInfo
                        {
                            LanguageIds = new ushort[totalLangs]
                        };

                        totalLangs = 0;
                        for (int i = 0; i < 10; i++)
                        {
                            if (descriptor->languageID[i] >= 0x0400 && descriptor->languageID[i] <= 0x0465)
                            {
                                result.LanguageIds[totalLangs++] = descriptor->languageID[i];
                            }
                        }
                    }
                    else
                    {
                        result = new StringInfo
                        {
                            LanguageIds = new ushort[0]
                        };
                    }

#if USB_TRACE
                    ShowString(result);
#endif
                }
            }
            finally
            {
                Heap.Free(descriptor);
            }

            return result;
        }

        /// <summary>
        ///     Gets a unicode string descriptor from the specified device.
        /// </summary>
        /// <param name="device">The device info of the device to get the descriptor from.</param>
        /// <param name="stringIndex">The index of the string descriptor to get.</param>
        /// <returns>True if USB transfer completed successfully. Otherwise, false.</returns>
        public static UnicodeString GetUnicodeStringDescriptor(USBDeviceInfo device, byte stringIndex)
        {
#if USB_TRACE
            DBGMSG(((FOS_System.String)"USB: GET_DESCRIPTOR string, endpoint: 0 stringIndex: ") + stringIndex);
#endif

            if (stringIndex == 0)
            {
                return new UnicodeString
                {
                    StringType = 0,
                    Value = "[NONE]"
                };
            }

            UnicodeString result = new UnicodeString
            {
                StringType = 0,
                Value = "[Failed to load]"
            };

            //64 byte buffer
            ushort bufferSize = 64;
            StringDescriptorUnicode* buffer =
                (StringDescriptorUnicode*) Heap.AllocZeroed(bufferSize, "USBManager : GetUnicodeStringDescriptor");

            try
            {
                USBTransfer transfer = new USBTransfer();
                device.hc.SetupTransfer(device, transfer, USBTransferType.Control, 0, bufferSize);
                device.hc.SETUPTransaction(transfer, 8, 0x80, 6, 3, stringIndex, 0x0409, bufferSize);
                device.hc.INTransaction(transfer, false, buffer, bufferSize);
                device.hc.OUTTransaction(transfer, true, null, 0);
                device.hc.IssueTransfer(transfer);

                if (transfer.success)
                {
                    result = new UnicodeString
                    {
                        StringType = buffer->descriptorType,
                        Value =
                            buffer->length > 0
                                ? ByteConverter.GetASCIIStringFromUTF16(buffer->widechar, 0, buffer->length)
                                : ""
                    };

#if USB_TRACE
                    ShowUnicodeString(result);
#endif
                }
            }
            finally
            {
                Heap.Free(buffer);
            }

            return result;
        }

        /// <summary>
        ///     Gets the current configuration number from the specified device.
        /// </summary>
        /// <param name="device">The device info of the device to get the information from.</param>
        /// <returns>The current configuration value.</returns>
        public static byte GetConfiguration(USBDeviceInfo device)
        {
#if USB_TRACE
            DBGMSG("USB: GET_CONFIGURATION");
#endif

            uint configuration;

            USBTransfer transfer = new USBTransfer();
            device.hc.SetupTransfer(device, transfer, USBTransferType.Control, 0, 64);
            device.hc.SETUPTransaction(transfer, 8, 0x80, 8, 0, 0, 0, 1);
            device.hc.INTransaction(transfer, false, &configuration, 1);
            device.hc.OUTTransaction(transfer, true, null, 0);
            device.hc.IssueTransfer(transfer);

            return (byte) configuration;
        }

        /// <summary>
        ///     Sets the current configuration number of the specified device.
        /// </summary>
        /// <param name="device">The device info of the device to set the information in.</param>
        /// <param name="configuration">The configuration number to set.</param>
        public static void SetConfiguration(USBDeviceInfo device, byte configuration)
        {
#if USB_TRACE
            DBGMSG(((FOS_System.String)"USB: SET_CONFIGURATION ") + configuration);
#endif

            USBTransfer transfer = new USBTransfer();
            device.hc.SetupTransfer(device, transfer, USBTransferType.Control, 0, 64);
            device.hc.SETUPTransaction(transfer, 8, 0x00, 9, 0, configuration, 0, 0);
            // SETUP DATA0, 8 byte, request type, SET_CONFIGURATION(9), hi(reserved), configuration, index=0, length=0
            device.hc.INTransaction(transfer, true, null, 0);
            device.hc.IssueTransfer(transfer);
        }

        /// <summary>
        ///     Gets the specified endpoint's status.
        /// </summary>
        /// <param name="device">The device info of the device to get the information from.</param>
        /// <param name="endpoint">The number of the endpoint to check the status of.</param>
        /// <returns>The status value.</returns>
        public static ushort GetStatus(USBDeviceInfo device, byte endpoint)
        {
#if USB_TRACE
            DBGMSG(((FOS_System.String)"USB: GetStatus, endpoint: ") + endpoint);
#endif

            ushort status;

            USBTransfer transfer = new USBTransfer();
            device.hc.SetupTransfer(device, transfer, USBTransferType.Control, endpoint, 64);
            device.hc.SETUPTransaction(transfer, 8, 0x02, 0, 0, 0, endpoint, 2);
            device.hc.INTransaction(transfer, false, &status, 2);
            device.hc.OUTTransaction(transfer, true, null, 0);
            device.hc.IssueTransfer(transfer);

#if USB_TRACE
            DBGMSG(((FOS_System.String)"Got status. Status=") + status + " for endpoint " + endpoint);
#endif

            return status;
        }

        /// <summary>
        ///     Sets the HALT feature on the specified endpoint.
        /// </summary>
        /// <param name="device">The device info of the device to which the endpoint belongs.</param>
        /// <param name="endpoint">The endpoint number on which to set the halt.</param>
        public static void SetFeatureHALT(USBDeviceInfo device, byte endpoint)
        {
#if USB_TRACE
            DBGMSG(((FOS_System.String)"USB: SetFeatureHALT, endpoint: ") + endpoint);
#endif

            USBTransfer transfer = new USBTransfer();
            device.hc.SetupTransfer(device, transfer, USBTransferType.Control, endpoint, 64);
            device.hc.SETUPTransaction(transfer, 8, 0x02, 3, 0, 0, endpoint, 0);
            device.hc.INTransaction(transfer, true, null, 0);
            device.hc.IssueTransfer(transfer);

#if USB_TRACE
            DBGMSG(((FOS_System.String)"Set HALT at endpoint: ") + endpoint);
#endif
        }

        /// <summary>
        ///     Clears the HALT feature on the specified endpoint.
        /// </summary>
        /// <param name="device">The device info of the device to which the endpoint belongs.</param>
        /// <param name="endpoint">The endpoint number from which to clear the halt.</param>
        public static void ClearFeatureHALT(USBDeviceInfo device, byte endpoint)
        {
#if USB_TRACE
            DBGMSG(((FOS_System.String)"USB: ClearFeatureHALT, endpoint: ") + endpoint);
#endif

            USBTransfer transfer = new USBTransfer();
            device.hc.SetupTransfer(device, transfer, USBTransferType.Control, endpoint, 64);
            device.hc.SETUPTransaction(transfer, 8, 0x02, 1, 0, 0, endpoint, 0);
            device.hc.INTransaction(transfer, true, null, 0);
            device.hc.IssueTransfer(transfer);

#if USB_TRACE
            DBGMSG(((FOS_System.String)"Cleared HALT at endpoint: ") + endpoint);
#endif
        }

        /// <remarks>
        ///     Called by Device Manager Task.
        /// </remarks>
        public static void IRQHandler()
        {
            if (HCIDevices != null)
            {
                for (int i = 0; i < HCIDevices.Count; i++)
                {
                    ((HCI) HCIDevices[i]).IRQHandler();
                }
            }
        }

#if USB_TRACE
        private static void ShowDevice(USBDeviceInfo usbDev)
        {
            if (usbDev.usbSpec == 0x0100 || usbDev.usbSpec == 0x0110 || usbDev.usbSpec == 0x0200 || usbDev.usbSpec == 0x0201 || usbDev.usbSpec == 0x0210 || usbDev.usbSpec == 0x0213 ||usbDev.usbSpec == 0x0300)
            {
                DBGMSG(((FOS_System.String)"USB ") + (byte)((usbDev.usbSpec >> 8) & 0xFF) + "." + (byte)(usbDev.usbSpec & 0xFF)); // e.g. 0x0210 means 2.10
            }
            else
            {
                DBGMSG(((FOS_System.String)"Invalid USB version ") + (byte)((usbDev.usbSpec >> 8) & 0xFF) + "." + (byte)(usbDev.usbSpec & 0xFF) + "!");
                //return;
            }

            if (usbDev.usbClass == 0x09)
            {
                switch (usbDev.usbProtocol)
                {
                    case 0:
                        DBGMSG(" - Full speed USB hub");
                        break;
                    case 1:
                        DBGMSG(" - Hi-speed USB hub with single TT");
                        break;
                    case 2:
                        DBGMSG(" - Hi-speed USB hub with multiple TTs");
                        break;
                }
            }

            DBGMSG(((FOS_System.String)"endpoint 0 mps: ") + ((Endpoint)usbDev.Endpoints[0]).MPS + " byte."); // MPS0, must be 8,16,32,64
            DBGMSG(((FOS_System.String)"vendor:            ") + usbDev.vendor);
            DBGMSG(((FOS_System.String)"product:           ") + usbDev.product);
            DBGMSG(((FOS_System.String)"release number:    ") + ((usbDev.releaseNumber >> 8) & 0xFF) + "." + (usbDev.releaseNumber & 0xFF));
            DBGMSG(((FOS_System.String)"manufacturer:      ") + usbDev.manufacturerStringID);
            DBGMSG(((FOS_System.String)"product:           ") + usbDev.productStringID);
            DBGMSG(((FOS_System.String)"serial number:     ") + usbDev.serialNumberStringID);
            DBGMSG(((FOS_System.String)"number of config.: ") + usbDev.numConfigurations); // number of possible configurations
            DBGMSG(((FOS_System.String)"MSDInterfaceNum:   ") + usbDev.MSD_InterfaceNum);
            BasicConsole.DelayOutput(5);
        }
        private static void ShowConfiguration(Configuration d)
        {
            DBGMSG(((FOS_System.String)"Number of interfaces: ") + d.NumInterfaces);
            DBGMSG(((FOS_System.String)"ID of config:         ") + d.Selector);
            if (d.Description != null)
            {
                DBGMSG(((FOS_System.String)"Description:          ") + d.Description.Value);
            }
            else
            {
                DBGMSG("Description:          [NONE]");
            }
            DBGMSG(((FOS_System.String)"Remote wakeup:        ") + (((d.Attribs & Configuration.Attributes.RemoteWakeup) != 0) ? "Yes" : "No"));
            DBGMSG(((FOS_System.String)"Self-powered:         ") + (((d.Attribs & Configuration.Attributes.SelfPowered) != 0) ? "Yes" : "No"));
            DBGMSG(((FOS_System.String)"Max power (mA):       ") + d.MaxPower * 2); // 2 mA steps used
            BasicConsole.DelayOutput(1);
        }
        private static void ShowInterface(Interface d)
        {
            DBGMSG("---------------------------------------------------------------------");
                
            switch (d.NumEndpoints)
            {
                case 0:
                    DBGMSG(((FOS_System.String)"Interface ") + d.InterfaceNumber + " has no endpoint and belongs to class:");
                    break;
                case 1:
                    DBGMSG(((FOS_System.String)"Interface ") + d.InterfaceNumber + " has only one endpoint and belongs to class:");
                    break;
                default:
                    DBGMSG(((FOS_System.String)"Interface ") + d.InterfaceNumber + " has " + d.NumEndpoints + " endpoints and belongs to class:");
                    break;
            }

            switch (d.Class)
            {
                case 0x01:
                    DBGMSG("Audio");
                    break;
                case 0x02:
                    DBGMSG("Communications and CDC Control");
                    break;
                case 0x03:
                    DBGMSG("HID (Human Interface Device)");
                    break;
                case 0x05:
                    DBGMSG("Physical");
                    break;
                case 0x06:
                    DBGMSG("Image");
                    break;
                case 0x07:
                    DBGMSG("Printer");
                    break;
                case 0x08:
                    DBGMSG("Mass Storage, ");
                    switch (d.Subclass)
                    {
                        case 0x01:
                            DBGMSG("Reduced Block Commands, ");
                            break;
                        case 0x02:
                            DBGMSG("SFF-8020i or MMC-2(ATAPI), ");
                            break;
                        case 0x03:
                            DBGMSG("QIC-157 (tape device), ");
                            break;
                        case 0x04:
                            DBGMSG("UFI (e.g. Floppy Disk), ");
                            break;
                        case 0x05:
                            DBGMSG("SFF-8070i (e.g. Floppy Disk), ");
                            break;
                        case 0x06:
                            DBGMSG("SCSI transparent command set, ");
                            break;
                    }
                    switch (d.Protocol)
                    {
                        case 0x00:
                            DBGMSG("CBI protocol with command completion interrupt.");
                            break;
                        case 0x01:
                            DBGMSG("CBI protocol without command completion interrupt.");
                            break;
                        case 0x50:
                            DBGMSG("Bulk-Only Transport protocol.");
                            break;
                    }
                    break;
                case 0x0A:
                    DBGMSG("CDC-Data");
                    break;
                case 0x0B:
                    DBGMSG("Smart Card");
                    break;
                case 0x0D:
                    DBGMSG("Content Security");
                    break;
                case 0x0E:
                    DBGMSG("Video");
                    break;
                case 0x0F:
                    DBGMSG("Personal Healthcare");
                    break;
                case 0xDC:
                    DBGMSG("Diagnostic Device");
                    break;
                case 0xE0:
                    DBGMSG(((FOS_System.String)"Wireless Controller, subclass: ") + d.Subclass + " protocol: " + d.Protocol + ".");
                    break;
                case 0xEF:
                    DBGMSG("Miscellaneous");
                    break;
                case 0xFE:
                    DBGMSG("Application Specific");
                    break;
                case 0xFF:
                    DBGMSG("Vendor Specific");
                    break;
            }

            DBGMSG(((FOS_System.String)"Alternate Setting:  ") + d.AlternateSetting);
            DBGMSG(((FOS_System.String)"Class:              ") + d.Class);
            DBGMSG(((FOS_System.String)"Subclass:           ") + d.Subclass);
            DBGMSG(((FOS_System.String)"Protocol:           ") + d.Protocol);
            if (d.Description != null)
            {
                DBGMSG(((FOS_System.String)"Description:        ") + d.Description.Value);
            }
            else
            {
                DBGMSG("Description:        [NONE]");
            }
            BasicConsole.DelayOutput(1);
        }
        private static void ShowEndpoint(Endpoint d)
        {
            DBGMSG("---------------------------------------------------------------------");
            DBGMSG(((FOS_System.String)"Endpoint ") +  d.Address + ": " + (d.Type == Endpoint.Types.BIDIR ? "Bidirectional" : 
                                                                           d.Type == Endpoint.Types.IN    ? "In" : 
                                                                           d.Type == Endpoint.Types.OUT   ? "Out" :
                                                                                                            "Uncrecognised!") + ", ");
            DBGMSG(((FOS_System.String)"Attributes: ") + d.Attributes);
            // bit 1:0 00 control    01 isochronous    10 bulk                         11 interrupt
            // bit 3:2 00 no sync    01 async          10 adaptive                     11 sync (only if isochronous)
            // bit 5:4 00 data endp. 01 feedback endp. 10 explicit feedback data endp. 11 reserved (Iso Mode)

            if ((d.Attributes & 0x3) == 2)
            {
                DBGMSG("Bulk endpoint,");
            }
            DBGMSG(((FOS_System.String)" MPS: ") + d.MPS + " bytes");
            DBGMSG(((FOS_System.String)" Interval: ") + d.Interval);
            BasicConsole.DelayOutput(1);
        }
        private static void ShowString(StringInfo info)
        {
            DBGMSG("Languages: ");
            for (byte i = 0; i < info.LanguageIds.Length; i++)
            {
                DBGMSG(StringInfo.GetLanguageName(info.LanguageIds[i]));
            }
        }
        private static void ShowUnicodeString(UnicodeString str)
        {
            DBGMSG(str.Value);
        }
        
        private static void DBGMSG(FOS_System.String msg)
        {
            BasicConsole.WriteLine(msg);
        }
#endif
    }
}