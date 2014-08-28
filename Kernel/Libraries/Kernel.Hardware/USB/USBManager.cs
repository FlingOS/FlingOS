#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
#define USB_TRACE
#undef USB_TRACE

using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.PCI;
using Kernel.Hardware.USB.HCIs;
using Kernel.Hardware.USB.Devices;
using Kernel.Utilities;

namespace Kernel.Hardware.USB
{
    /// <summary>
    /// Provides methods for managing USB access.
    /// </summary>
    public static unsafe class USBManager
    {
        /// <summary>
        /// The number of UHCI devices detected.
        /// </summary>
        public static int NumUHCIDevices
        {
            get;
            private set;
        }
        /// <summary>
        /// The number of OHCI devices detected.
        /// </summary>
        public static int NumOHCIDevices
        {
            get;
            private set;
        }
        /// <summary>
        /// The number of EHCI devices detected.
        /// </summary>
        public static int NumEHCIDevices
        {
            get;
            private set;
        }
        /// <summary>
        /// The number of xHCI devices detected.
        /// </summary>
        public static int NumxHCIDevices
        {
            get;
            private set;
        }

        /// <summary>
        /// List of all the HCI device instances.
        /// </summary>
        public static List HCIDevices = new List(1);
        /// <summary>
        /// List of all the USB device instances.
        /// </summary>
        public static List Devices = new List(5);
        
        /// <summary>
        /// Initialises USB management. Scans the PCI bus for HCIs and initialises any supported HCIs that are found.
        /// </summary>
        public static void Init()
        {
            //Enumerate PCI devices looking for (unclaimed) USB host controllers

            //      UHCI:  Class ID: 0x0C, Sub-class: 0x03, Prog(ramming) Interface: 0x00
            //      OHCI:  Class ID: 0x0C, Sub-class: 0x03, Prog(ramming) Interface: 0x10
            //      EHCI:  Class ID: 0x0C, Sub-class: 0x03, Prog(ramming) Interface: 0x20
            //      xHCI:  Class ID: 0x0C, Sub-class: 0x03, Prog(ramming) Interface: 0x30

            //TODO - Check host controllers haven't already been claimed / initialised!

            //            for (int i = 0; i < PCI.PCI.Devices.Count; i++)
            //            {
            //                PCIDevice aDevice = (PCIDevice)(PCI.PCI.Devices[i]);
            //                //0x0C = Serial bus controllers
            //                if (aDevice.ClassCode == 0x0C)
            //                {
            //                    //0x03 = USB controllers
            //                    if (aDevice.Subclass == 0x03)
            //                    {
            //                        //xHCI = 0x30
            //                        if (aDevice.ProgIF == 0x30)
            //                        {
            //                            //xHCI detected
            //#if USB_TRACE
            //                            BasicConsole.WriteLine("xHCI detected.");
            //#endif

            //                            //TODO - Add xHCI support
            //                            //Supported by VMWare
            //                            //  - This is USB 3.0

            //                            NumxHCIDevices++;
            //                        }
            //                    }
            //                }
            //            }
            for (int i = 0; i < PCI.PCI.Devices.Count; i++)
            {
                PCIDevice aDevice = (PCIDevice)(PCI.PCI.Devices[i]);
                //0x0C = Serial bus controllers
                if (aDevice.ClassCode == 0x0C)
                {
                    //0x03 = USB controllers
                    if (aDevice.Subclass == 0x03)
                    {
                        //EHCI = 0x20
                        if (aDevice.ProgIF == 0x20)
                        {
                            //EHCI detected
#if USB_TRACE
                            BasicConsole.WriteLine("EHCI detected.");
#endif
                            NumEHCIDevices++;

                            PCIDeviceNormal EHCI_PCIDevice = (PCIDeviceNormal)aDevice;
                            EHCI_PCIDevice.Claimed = true;

                            EHCI newEHCI = new EHCI(EHCI_PCIDevice);

                            HCIDevices.Add(newEHCI);
                            DeviceManager.Devices.Add(newEHCI);

#if USB_TRACE
                            BasicConsole.DelayOutput(10);
#endif
                        }
                    }
                }
            }
            //            for (int i = 0; i < PCI.PCI.Devices.Count; i++)
            //            {
            //                PCIDevice aDevice = (PCIDevice)(PCI.PCI.Devices[i]);
            //                //0x0C = Serial bus controllers
            //                if (aDevice.ClassCode == 0x0C)
            //                {
            //                    //0x03 = USB controllers
            //                    if (aDevice.Subclass == 0x03)
            //                    {
            //                        //UHCI = 0x00
            //                        if (aDevice.ProgIF == 0x00)
            //                        {
            //                            //UHCI detected
            //#if USB_TRACE
            //                            BasicConsole.WriteLine("UHCI detected.");
            //#endif
            //                            NumUHCIDevices++;

            //                            PCIDeviceNormal UHCI_PCIDevice = (PCIDeviceNormal)aDevice;
            //                            UHCI_PCIDevice.Claimed = true;

            //                            UHCI newUHCI = new UHCI(UHCI_PCIDevice);

            //                            HCIDevices.Add(newUHCI);
            //                            DeviceManager.Devices.Add(newUHCI);

            //                            BasicConsole.DelayOutput(10);
            //                        }
            //                        //OHCI = 0x10
            //                        else if (aDevice.ProgIF == 0x10)
            //                        {
            //                            //OHCI detected
            //#if USB_TRACE
            //                            BasicConsole.WriteLine("OHCI detected.");
            //#endif

            //                            //TODO - Add OHCI support
            //                            //Not supported by VMWare or my laptop 
            //                            //  so we aren't going to program this any further for now.

            //                            NumOHCIDevices++;
            //                        }
            //                    }
            //                }
            //            }
        }
        /// <summary>
        /// Updates the USb manager and all host controller devices.
        /// </summary>
        public static void Update()
        {
            for(int i = 0; i < HCIDevices.Count; i++)
            {
                ((HCI)HCIDevices[i]).Update();
            }
        }

        /// <summary>
        /// Creates new device info for the specified port.
        /// </summary>
        /// <param name="hc">The host contorller which owns the port.</param>
        /// <param name="port">The port to create device info for.</param>
        /// <returns>The new device info.</returns>
        public static Devices.USBDeviceInfo CreateDeviceInfo(HCI hc, HCPort port)
        {
#if USB_TRACE
            DBGMSG("Creating USB device...");
#endif
            USBDeviceInfo deviceInf = new USBDeviceInfo(port.portNum, hc);
            deviceInf.Endpoints = new List(1);
            deviceInf.Endpoints.Add(new Endpoint());
            ((Endpoint)deviceInf.Endpoints[0]).mps = 64;
            ((Endpoint)deviceInf.Endpoints[0]).type = EndpointType.BIDIR;
            ((Endpoint)deviceInf.Endpoints[0]).toggle = false;
#if USB_TRACE
            DBGMSG("Created device.");
#endif
            return deviceInf;
        }
        /// <summary>
        /// Creates and initialises a new USb device for the specified device info and address.
        /// </summary>
        /// <param name="deviceInfo">The device info to create a new device for.</param>
        /// <param name="address">The USB address for the new device.</param>
        public static void SetupDevice(USBDeviceInfo deviceInfo, byte address)
        {
            deviceInfo.address = 0; // device number has to be set to 0
            bool success = false;

            try
            {
                success = GetDeviceDescriptor(deviceInfo);
                if (!success)
                {
                    success = GetDeviceDescriptor(deviceInfo);
                }
                FOS_System.GC.Cleanup();

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

                bool hub = deviceInfo.usbClass == 0x09;

                deviceInfo.address = SetDeviceAddress(deviceInfo, address);
                FOS_System.GC.Cleanup();
#if USB_TRACE
                if (hub)
                {
                    DBGMSG(" <-- usb Hub");
                    BasicConsole.DelayOutput(2);
                }
#endif

                success = GetConfigDescriptor(deviceInfo);
                if (!success)
                {
                    success = GetConfigDescriptor(deviceInfo);
                }
                FOS_System.GC.Cleanup();

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
                
#if USB_TRACE
                if (!hub)
                {
                    GetStringDescriptor(deviceInfo);
                    FOS_System.GC.Cleanup();
                    BasicConsole.DelayOutput(2);

                    for (byte i = 1; i < 4; i++) // Fetch descriptor strings 1, 2
                    {
                        GetUnicodeStringDescriptor(deviceInfo, i);
                    }
                }
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

                if (hub)
                {
//TODO: Uncomment these when hub driver done
//#if USB_TRACE
                    BasicConsole.WriteLine("-------------------------- Hub --------------------------");
                    BasicConsole.DelayOutput(2);
//#endif

                    //TODO: Hub driver
                    //success = GetHubDescriptor(device);
                    //if (success)
                    //{
                    //    printf("\nThe hub owns %u downstream ports.", ((usb_hubDescriptor_t*)device->data)->numberPorts);
                    //    usb_setupHub(device);
                    //}
                    //else
                    //{
                    //    textColor(ERROR);
                    //    printf("\nHubDescriptor could not be read!");
                    //    textColor(TEXT);
                    //}
                    //waitForKeyStroke();

                    //For now, create a completely generic device instance so we don't lose track of
                    //  this device entirely.
                    new USBDevice(deviceInfo);
                }
                else if (deviceInfo.InterfaceClass == 0x08)
                {
#if USB_TRACE
                    DBGMSG("------------------ Mass Storage Device ------------------");
                    BasicConsole.DelayOutput(2);
#endif
                    try
                    {
                        new MassStorageDevice(deviceInfo);
                    }
                    catch
                    {
#if USB_TRACE
                        DBGMSG("Error creating USb device! Aborted creating device.");
#endif
                    }
                }
                else
                {
#if USB_TRACE
                    DBGMSG("Unrecognised USB device detected.");
                    BasicConsole.DelayOutput(2);
#endif
                    //For now, create a completely generic device instance so we don't lose track of
                    //  this device entirely.
                    new USBDevice(deviceInfo);
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
        /// Sets the USB device address of the specified device to the specified value.
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
            DBGMSG(((FOS_System.String)"new address: ") + new_address);
            BasicConsole.DelayOutput(4);
#endif
            return new_address;
        }
        
        /// <summary>
        /// Gets the device descriptor from the specified device.
        /// </summary>
        /// <param name="device">The device info of the device to get the descriptor from.</param>
        /// <returns>True if USB transfer completed successfully. Otherwise, false.</returns>
        public static bool GetDeviceDescriptor(USBDeviceInfo device)
        {
#if USB_TRACE
            DBGMSG("USB: GET_DESCRIPTOR Device");
#endif

            DeviceDescriptor* descriptor = (DeviceDescriptor*)FOS_System.Heap.AllocZeroed((uint)sizeof(DeviceDescriptor));
            USBTransfer transfer = new USBTransfer();
            try
            {
                device.hc.SetupTransfer(device, transfer, USBTransferType.Control, 0, 64);
                device.hc.SETUPTransaction(transfer, 8, 0x80, 6, 1, 0, 0, 18);
                device.hc.INTransaction(transfer, false, descriptor, 18);
                device.hc.OUTTransaction(transfer, true, null, 0);
                device.hc.IssueTransfer(transfer);

                if (transfer.success)
                {
#if EHCI_TRACE || USB_TRACE
                    byte* bpDescriptor = (byte*)descriptor;
                    for (int i = 0; i < sizeof(USBDeviceDescriptor); i++)
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
                FOS_System.Heap.Free(descriptor);
            }
            return transfer.success;
        }
        /// <summary>
        /// Parses the specified device descriptor. 
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
            usbDev.serNumberStringID = d->serialNumber;
            usbDev.numConfigurations = d->numConfigurations;
            ((Endpoint)usbDev.Endpoints[0]).mps = d->MaxPacketSize;
        }

        /// <summary>
        /// Gets the configuration descriptor from the specified device.
        /// </summary>
        /// <param name="device">The device info of the device to get the descriptor from.</param>
        /// <returns>True if USB transfer completed successfully. Otherwise, false.</returns>
        public static unsafe bool GetConfigDescriptor(USBDeviceInfo device)
        {
#if USB_TRACE
            DBGMSG("USB: GET_DESCRIPTOR Config");
#endif

            //64 byte buffer
            ushort bufferSize = 64;
            byte* buffer = (byte*)FOS_System.Heap.AllocZeroed(bufferSize);

            USBTransfer transfer = new USBTransfer();
            device.hc.SetupTransfer(device, transfer, USBTransferType.Control, 0, bufferSize);
            device.hc.SETUPTransaction(transfer, 8, 0x80, 6, 2, 0, 0, bufferSize);
            device.hc.INTransaction(transfer, false, buffer, 64);
            device.hc.OUTTransaction(transfer, true, null, 0);
            device.hc.IssueTransfer(transfer);

            if (transfer.success)
            {
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
                        ConfigurationDescriptor* descriptor = (ConfigurationDescriptor*)addr;
#if USB_TRACE
                        ShowConfigurationDescriptor(descriptor);
#endif
                    }
                    else if (length == 9 && type == 4)
                    {
                        InterfaceDescriptor* descriptor = (InterfaceDescriptor*)addr;
#if USB_TRACE
                        ShowInterfaceDescriptor(descriptor);
#endif

                        if (descriptor->interfaceClass == 8)
                        {
                            // store interface number for mass storage transfers
                            device.MSD_InterfaceNum = descriptor->interfaceNumber;
                            device.InterfaceClass = descriptor->interfaceClass;
                            device.InterfaceSubclass = descriptor->interfaceSubclass;
                        }
                        numEndpoints += descriptor->numEndpoints;
                    }
                    else if (length == 7 && type == 5)
                    {
                    }
                    else
                    {
#if USB_TRACE
                        DBGMSG(((FOS_System.String)"length: ") + length + " type: " + type + " - unknown");
#endif
                        if (length == 0)
                        {
                            break;
                        }
                    }
                    addr += length;
                }

                for (int i = 1; i < device.Endpoints.Count; i++)
                {
                    device.Endpoints.RemoveAt(i);
                }
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
                        EndpointDescriptor* descriptor = (EndpointDescriptor*)addr;
#if USB_TRACE
                        ShowEndpointDescriptor(descriptor);
#endif

                        byte ep_id = (byte)(descriptor->endpointAddress & 0xF);
#if USB_TRACE
                        if (ep_id >= numEndpoints)
                        {
                            DBGMSG("ep_id >= numEndpoints!!");
                        }
#endif

                        ((Endpoint)device.Endpoints[ep_id]).mps = descriptor->maxPacketSize;
                        ((Endpoint)device.Endpoints[ep_id]).type = EndpointType.BIDIR; // Can be overwritten below
                        
                        // store endpoint numbers for IN/OUT mass storage transfers, attributes must be 0x2, because there are also endpoints with attributes 0x3(interrupt)
                        if ((descriptor->endpointAddress & 0x80) > 0 && descriptor->attributes == 0x2)
                        {
                            if (ep_id < 3)
                            {
                                device.MSD_INEndpointID = ep_id;
                            }
                            ((Endpoint)device.Endpoints[ep_id]).type = EndpointType.IN;
                        }

                        if ((descriptor->endpointAddress & 0x80) == 0 && descriptor->attributes == 0x2)
                        {
                            if (ep_id < 3)
                            {
                                device.MSD_OUTEndpointID = ep_id;
                            }
                            ((Endpoint)device.Endpoints[ep_id]).type = EndpointType.OUT;
                        }
                    }
                    else if (length == 0)
                    {
                        break;
                    }

                    addr += length;
                }
            }

            return transfer.success;
        }

        /// <summary>
        /// Gets the device string descriptor from the specified device.
        /// </summary>
        /// <param name="device">The device info of the device to get the descriptor from.</param>
        /// <returns>True if USB transfer completed successfully. Otherwise, false.</returns>
        public static void GetDeviceStringDescriptor(USBDeviceInfo device)
        {
#if USB_TRACE
            DBGMSG("USB: GET_DESCRIPTOR string");
#endif

            StringDescriptor* descriptor = (StringDescriptor*)FOS_System.Heap.AllocZeroed((uint)sizeof(StringDescriptor));

            try
            {
                ushort size = (ushort)sizeof(StringDescriptor);
                USBTransfer transfer = new USBTransfer();
                device.hc.SetupTransfer(device, transfer, USBTransferType.Control, 0, 64);
                device.hc.SETUPTransaction(transfer, 8, 0x80, 6, 3, 0, 0, size);
                device.hc.INTransaction(transfer, false, descriptor, size);
                device.hc.OUTTransaction(transfer, true, null, 0);
                device.hc.IssueTransfer(transfer);
                
#if USB_TRACE
                if (transfer.success)
                {
                    ShowStringDescriptor(descriptor);
                }
#endif
            }
            finally
            {
                FOS_System.Heap.Free(descriptor);
            }
        }

        /// <summary>
        /// Gets a unicode string descriptor from the specified device.
        /// </summary>
        /// <param name="device">The device info of the device to get the descriptor from.</param>
        /// <param name="stringIndex">The index of the string descriptor to get.</param>
        /// <returns>True if USB transfer completed successfully. Otherwise, false.</returns>
        public static void GetUnicodeStringDescriptor(USBDeviceInfo device, byte stringIndex)
        {
#if USB_TRACE
            DBGMSG(((FOS_System.String)"USB: GET_DESCRIPTOR string, endpoint: 0 stringIndex: ") + stringIndex);
#endif

            //64 byte buffer
            ushort bufferSize = 64;
            byte* buffer = (byte*)FOS_System.Heap.AllocZeroed(bufferSize);

            USBTransfer transfer = new USBTransfer();
            device.hc.SetupTransfer(device, transfer, USBTransferType.Control, 0, bufferSize);
            device.hc.SETUPTransaction(transfer, 8, 0x80, 6, 3, stringIndex, 0x0409, bufferSize);
            device.hc.INTransaction(transfer, false, buffer, bufferSize);
            device.hc.OUTTransaction(transfer, true, null, 0);
            device.hc.IssueTransfer(transfer);

#if USB_TRACE
            ShowUnicodeStringDescriptor((usb_stringDescriptorUnicode*)buffer, device, stringIndex);
#endif
        }

        /// <summary>
        /// Gets the current configuration number from the specified device.
        /// </summary>
        /// <param name="device">The device info of the device to get the information from.</param>
        /// <returns>The current configuration value.</returns>
        public static unsafe byte GetConfiguration(USBDeviceInfo device)
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

            return (byte)configuration;
        }
        /// <summary>
        /// Sets the current configuration number of the specified device.
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
            device.hc.SETUPTransaction(transfer, 8, 0x00, 9, 0, configuration, 0, 0); // SETUP DATA0, 8 byte, request type, SET_CONFIGURATION(9), hi(reserved), configuration, index=0, length=0
            device.hc.INTransaction(transfer, true, null, 0);
            device.hc.IssueTransfer(transfer);
        }
        
        /// <summary>
        /// Gets the specified endpoint's status.
        /// </summary>
        /// <param name="device">The device info of the device to get the information from.</param>
        /// <param name="endpoint">The number of the endpoint to check the status of.</param>
        /// <returns>The status value.</returns>
        public static unsafe ushort GetStatus(USBDeviceInfo device, byte endpoint)
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
        /// Sets the HALT feature on the specified endpoint.
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
        /// Clears the HALT feature on the specified endpoint.
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

            DBGMSG(((FOS_System.String)"endpoint 0 mps: ") + ((USBEndpoint)usbDev.Endpoints[0]).mps + " byte."); // MPS0, must be 8,16,32,64
            DBGMSG(((FOS_System.String)"vendor:            ") + usbDev.vendor);
            DBGMSG(((FOS_System.String)"product:           ") + usbDev.product);
            DBGMSG(((FOS_System.String)"release number:    ") + ((usbDev.releaseNumber >> 8) & 0xFF) + "." + (usbDev.releaseNumber & 0xFF));
            DBGMSG(((FOS_System.String)"manufacturer:      ") + usbDev.manufacturerStringID);
            DBGMSG(((FOS_System.String)"product:           ") + usbDev.productStringID);
            DBGMSG(((FOS_System.String)"serial number:     ") + usbDev.serNumberStringID);
            DBGMSG(((FOS_System.String)"number of config.: ") + usbDev.numConfigurations); // number of possible configurations
            DBGMSG(((FOS_System.String)"MSDInterfaceNum:   ") + usbDev.MSD_InterfaceNum);
            BasicConsole.DelayOutput(5);
        }
        private static void ShowConfigurationDescriptor(usb_configurationDescriptor* d)
        {
            if (d->length > 0)
            {
                DBGMSG(((FOS_System.String)"length:               ") + d->length);
                DBGMSG(((FOS_System.String)"descriptor type:      ") + d->descriptorType);
                DBGMSG(((FOS_System.String)"total length:         ") + d->totalLength);
                DBGMSG(((FOS_System.String)"Number of interfaces: ") + d->numInterfaces);
                DBGMSG(((FOS_System.String)"ID of config:         ") + d->configurationValue);
                DBGMSG(((FOS_System.String)"ID of config name     ") + d->configuration);
                DBGMSG(((FOS_System.String)"remote wakeup:        ") + (((d->attributes & Utilities.ConstantsUtils.BIT(5)) > 0) ? "yes" : "no"));
                DBGMSG(((FOS_System.String)"self-powered:         ") + (((d->attributes & Utilities.ConstantsUtils.BIT(6)) > 0) ? "yes" : "no"));
                DBGMSG(((FOS_System.String)"max power (mA):       ") + d->maxPower * 2); // 2 mA steps used
                BasicConsole.DelayOutput(1);
            }
        }
        private static void ShowInterfaceDescriptor(usb_interfaceDescriptor* d)
        {
            if (d->length > 0)
            {
                DBGMSG("---------------------------------------------------------------------");
                DBGMSG(((FOS_System.String)"length:               ") + d->length);          // 9
                DBGMSG(((FOS_System.String)"descriptor type:      ") + d->descriptorType);  // 4

                switch (d->numEndpoints)
                {
                    case 0:
                        DBGMSG(((FOS_System.String)"Interface ") + d->interfaceNumber + " has no endpoint and belongs to class:");
                        break;
                    case 1:
                        DBGMSG(((FOS_System.String)"Interface ") + d->interfaceNumber + " has only one endpoint and belongs to class:");
                        break;
                    default:
                        DBGMSG(((FOS_System.String)"Interface ") + d->interfaceNumber + " has " + d->numEndpoints + " endpoints and belongs to class:");
                        break;
                }

                switch (d->interfaceClass)
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
                        switch (d->interfaceSubclass)
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
                        switch (d->interfaceProtocol)
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
                        DBGMSG(((FOS_System.String)"Wireless Controller, subclass: ") + d->interfaceSubclass + " protocol: " + d->interfaceProtocol + ".");
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

                DBGMSG(((FOS_System.String)"alternate Setting:  ") + d->alternateSetting);
                DBGMSG(((FOS_System.String)"interface class:      ") + d->interfaceClass);
                DBGMSG(((FOS_System.String)"interface subclass:   ") + d->interfaceSubclass);
                DBGMSG(((FOS_System.String)"interface protocol:   ") + d->interfaceProtocol);
                DBGMSG(((FOS_System.String)"interface:            ") + d->Interface);
                BasicConsole.DelayOutput(1);
            }
        }
        private static void ShowEndpointDescriptor(usb_endpointDescriptor* d)
        {
            if (d->length != 0)
            {
                DBGMSG("---------------------------------------------------------------------");
                DBGMSG(((FOS_System.String)"length:      ") + d->length);         // 7
                DBGMSG(((FOS_System.String)"descriptor type: ") + d->descriptorType); // 5
                DBGMSG(((FOS_System.String)"endpoint ") +  (d->endpointAddress & 0xF) + ": " + ((d->endpointAddress & 0x80) != 0 ? "IN " : "OUT") + ", ");
                DBGMSG(((FOS_System.String)"attributes: ") + d->attributes);
                // bit 1:0 00 control    01 isochronous    10 bulk                         11 interrupt
                // bit 3:2 00 no sync    01 async          10 adaptive                     11 sync (only if isochronous)
                // bit 5:4 00 data endp. 01 feedback endp. 10 explicit feedback data endp. 11 reserved (Iso Mode)

                if (d->attributes == 2)
                {
                    DBGMSG("bulk data,");
                }
                DBGMSG(((FOS_System.String)" mps: ") + d->maxPacketSize + " bytes");
                DBGMSG(((FOS_System.String)" interval: ") + d->interval);
                BasicConsole.DelayOutput(1);
            }
        }
        private static void ShowStringDescriptor(usb_stringDescriptor* d)
        {
            if (d->length != 0)
            {
                //DBGMSG("length:          %u\t", d->length);         // 12
                //DBGMSG("descriptor type: %u\n", d->descriptorType); //  3

                DBGMSG("Languages: ");
                for (byte i = 0; i < 10; i++)
                {
                    if (d->languageID[i] >= 0x0400 && d->languageID[i] <= 0x0465)
                    {
                        switch (d->languageID[i])
                        {
                            case 0x400:
                                DBGMSG("Neutral");
                                break;
                            case 0x401:
                                DBGMSG("Arabic");
                                break;
                            case 0x402:
                                DBGMSG("Bulgarian");
                                break;
                            case 0x403:
                                DBGMSG("Catalan");
                                break;
                            case 0x404:
                                DBGMSG("Chinese");
                                break;
                            case 0x405:
                                DBGMSG("Czech");
                                break;
                            case 0x406:
                                DBGMSG("Danish");
                                break;
                            case 0x407:
                                DBGMSG("German");
                                break;
                            case 0x408:
                                DBGMSG("Greek");
                                break;
                            case 0x409:
                                DBGMSG("English");
                                break;
                            case 0x40a:
                                DBGMSG("Spanish");
                                break;
                            case 0x40b:
                                DBGMSG("Finnish");
                                break;
                            case 0x40c:
                                DBGMSG("French");
                                break;
                            case 0x40d:
                                DBGMSG("Hebrew");
                                break;
                            case 0x40e:
                                DBGMSG("Hungarian");
                                break;
                            case 0x40f:
                                DBGMSG("Icelandic");
                                break;
                            case 0x410:
                                DBGMSG("Italian");
                                break;
                            case 0x411:
                                DBGMSG("Japanese");
                                break;
                            case 0x412:
                                DBGMSG("Korean");
                                break;
                            case 0x413:
                                DBGMSG("Dutch");
                                break;
                            case 0x414:
                                DBGMSG("Norwegian");
                                break;
                            case 0x415:
                                DBGMSG("Polish");
                                break;
                            case 0x416:
                                DBGMSG("Portuguese");
                                break;
                            case 0x418:
                                DBGMSG("Romanian");
                                break;
                            case 0x419:
                                DBGMSG("Russian");
                                break;
                            case 0x41a:
                                DBGMSG("Croatian");
                                break;
                            //case 0x41a: - Same as previous...hmm...
                                //TODO - Find out the actual language code for Serbian or Croatian (whichever is wrong)
                            //    DBGMSG("Serbian");
                            //    break; 
                            case 0x41b:
                                DBGMSG("Slovak");
                                break;
                            case 0x41c:
                                DBGMSG("Albanian");
                                break;
                            case 0x41d:
                                DBGMSG("Swedish");
                                break;
                            case 0x41e:
                                DBGMSG("Thai");
                                break;
                            case 0x41f:
                                DBGMSG("Turkish");
                                break;
                            case 0x420:
                                DBGMSG("Urdu");
                                break;
                            case 0x421:
                                DBGMSG("Indonesian");
                                break;
                            case 0x422:
                                DBGMSG("Ukrainian");
                                break;
                            case 0x423:
                                DBGMSG("Belarusian");
                                break;
                            case 0x424:
                                DBGMSG("Slovenian");
                                break;
                            case 0x425:
                                DBGMSG("Estonian");
                                break;
                            case 0x426:
                                DBGMSG("Latvian");
                                break;
                            case 0x427:
                                DBGMSG("Lithuanian");
                                break;
                            case 0x429:
                                DBGMSG("Farsi");
                                break;
                            case 0x42a:
                                DBGMSG("Vietnamese");
                                break;
                            case 0x42b:
                                DBGMSG("Armenian");
                                break;
                            case 0x42c:
                                DBGMSG("Azeri");
                                break;
                            case 0x42d:
                                DBGMSG("Basque");
                                break;
                            case 0x42f:
                                DBGMSG("Macedonian");
                                break;
                            case 0x436:
                                DBGMSG("Afrikaans");
                                break;
                            case 0x437:
                                DBGMSG("Georgian");
                                break;
                            case 0x438:
                                DBGMSG("Faeroese");
                                break;
                            case 0x439:
                                DBGMSG("Hindi");
                                break;
                            case 0x43e:
                                DBGMSG("Malay");
                                break;
                            case 0x43f:
                                DBGMSG("Kazak");
                                break;
                            case 0x440:
                                DBGMSG("Kyrgyz");
                                break;
                            case 0x441:
                                DBGMSG("Swahili");
                                break;
                            case 0x443:
                                DBGMSG("Uzbek");
                                break;
                            case 0x444:
                                DBGMSG("Tatar");
                                break;
                            case 0x446:
                                DBGMSG("Punjabi");
                                break;
                            case 0x447:
                                DBGMSG("Gujarati");
                                break;
                            case 0x449:
                                DBGMSG("Tamil");
                                break;
                            case 0x44a:
                                DBGMSG("Telugu");
                                break;
                            case 0x44b:
                                DBGMSG("Kannada");
                                break;
                            case 0x44e:
                                DBGMSG("Marathi");
                                break;
                            case 0x44f:
                                DBGMSG("Sanskrit");
                                break;
                            case 0x450:
                                DBGMSG("Mongolian");
                                break;
                            case 0x456:
                                DBGMSG("Galician");
                                break;
                            case 0x457:
                                DBGMSG("Konkani");
                                break;
                            case 0x45a:
                                DBGMSG("Syriac");
                                break;
                            case 0x465:
                                DBGMSG("Divehi");
                                break;
                                break;
                            default:
                                DBGMSG(((FOS_System.String)"Language code: ") + d->languageID[i]);
                                break;
                        }
                    }
                }
            }
        }
        private static void ShowUnicodeStringDescriptor(usb_stringDescriptorUnicode* d, USBDeviceInfo device, uint stringIndex)
        {
            if (d->length != 0)
            {
                DBGMSG(((FOS_System.String)"length:          ") + d->length);
                DBGMSG(((FOS_System.String)"descriptor type: ") + d->descriptorType);
                DBGMSG(FOS_System.ByteConverter.GetASCIIStringFromUTF16((byte*)d->widechar, 0, d->length));
            }
        }
        
        private static void DBGMSG(FOS_System.String msg)
        {
            BasicConsole.WriteLine(msg);
        }
#endif
    }
}
