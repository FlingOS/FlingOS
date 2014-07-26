#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion

using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.PCI;
using Kernel.Hardware.USB.HCIs;
using Kernel.Hardware.USB.Devices;

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
        public static int NumUSBDevices = 0;

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

            for (int i = 0; i < PCI.PCI.Devices.Count; i++)
            {
                PCIDevice aDevice = (PCIDevice)(PCI.PCI.Devices[i]);
                //0x0C = Serial bus controllers
                if (aDevice.ClassCode == 0x0C)
                {
                    //0x03 = USB controllers
                    if (aDevice.Subclass == 0x03)
                    {
                        //xHCI = 0x30
                        if (aDevice.ProgIF == 0x30)
                        {
                            //xHCI detected
#if DEBUG
                            BasicConsole.WriteLine("xHCI detected.");
#endif

                            //TODO - Add xHCI support
                            //Supported by VMWare
                            //  - This is USB 3.0

                            NumxHCIDevices++;
                        }
                    }
                }
            }
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
#if DEBUG
                            BasicConsole.WriteLine("EHCI detected.");
#endif
                            NumEHCIDevices++;

                            PCIDeviceNormal EHCI_PCIDevice = (PCIDeviceNormal)aDevice;
                            EHCI_PCIDevice.Claimed = true;

                            EHCI newEHCI = new EHCI(EHCI_PCIDevice);

                            HCIDevices.Add(newEHCI);
                            DeviceManager.Devices.Add(newEHCI);

                            BasicConsole.DelayOutput(10);
                        }
                    }
                }
            }
            for (int i = 0; i < PCI.PCI.Devices.Count; i++)
            {
                PCIDevice aDevice = (PCIDevice)(PCI.PCI.Devices[i]);
                //0x0C = Serial bus controllers
                if (aDevice.ClassCode == 0x0C)
                {
                    //0x03 = USB controllers
                    if (aDevice.Subclass == 0x03)
                    {
                        //UHCI = 0x00
                        if (aDevice.ProgIF == 0x00)
                        {
                            //UHCI detected
#if DEBUG
                            BasicConsole.WriteLine("UHCI detected.");
#endif
                            NumUHCIDevices++;

                            PCIDeviceNormal UHCI_PCIDevice = (PCIDeviceNormal)aDevice;
                            UHCI_PCIDevice.Claimed = true;

                            UHCI newUHCI = new UHCI(UHCI_PCIDevice);

                            HCIDevices.Add(newUHCI);
                            DeviceManager.Devices.Add(newUHCI);

                            BasicConsole.DelayOutput(10);
                        }
                        //OHCI = 0x10
                        else if (aDevice.ProgIF == 0x10)
                        {
                            //OHCI detected
#if DEBUG
                            BasicConsole.WriteLine("OHCI detected.");
#endif

                            //TODO - Add OHCI support
                            //Not supported by VMWare or my laptop 
                            //  so we aren't going to program this any further for now.

                            NumOHCIDevices++;
                        }
                    }
                }
            }
        }

        public static Devices.USBDevice CreateDevice(HCI hc, HCPort port)
        {
#if DEBUG
            DBGMSG("Creating USB device...");
#endif
            USBDevice device = new USBDevice()
            {
                hc = hc
            };
            device.Endpoints = new List(1);
            device.Endpoints.Add(new USBEndpoint());
            ((USBEndpoint)device.Endpoints[0]).mps = 64;
            ((USBEndpoint)device.Endpoints[0]).type = USBEndpointType.EP_BIDIR;
            device.SerialNumber = "             ";
            ((USBEndpoint)device.Endpoints[0]).toggle = false;
#if DEBUG
            DBGMSG("Created device.");
#endif
            return device;
        }
        public static void DestroyDevice(USBDevice device)
        {
            device.Destroy();
        }
        public static void SetupDevice(USBDevice device, byte address)
        {
            device.num = 0; // device number has to be set to 0
            bool success = false;

            success = GetDeviceDescriptor(device);
            if (!success)
            {
                success = GetDeviceDescriptor(device);
            }

            if (!success)
            {
#if DEBUG
                DBGMSG("Setup Device interrupted!");
#endif
                return;
            }

            bool hub = device.usbClass == 0x09;

            device.num = SetDeviceAddress(device, address);
#if DEBUG
            DBGMSG(((FOS_System.String)"usb-Device address: ") + device.num);

            if (hub)
            {
                DBGMSG(" <-- usb Hub");
            }
#endif

            success = GetConfigDescriptor(device);
            if (!success)
            {
                success = GetConfigDescriptor(device);
            }

            if (!success)
            {
#if DEBUG
                DBGMSG("ConfigDescriptor could not be read! Setup Device interrupted!");
#endif
                return;
            }

            if (!hub)
            {
                GetStringDescriptor(device);
#if DEBUG
                BasicConsole.DelayOutput(4);
#endif

                for (byte i = 1; i < 4; i++) // fetch 3 strings
                {
                    GetUnicodeStringDescriptor(device, i);
                }
            }

            SetConfiguration(device, 1); // set first configuration

#if DEBUG
            byte config = GetConfiguration(device);
            DBGMSG(((FOS_System.String)"configuration: ") + config); // check configuration
            BasicConsole.DelayOutput(4);
#endif

            if (hub)
            {
#if DEBUG
                DBGMSG("This is a hub.");
#endif
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
                BasicConsole.DelayOutput(1);
            }
            else if (device.InterfaceClass == 0x08)
            {
                //TODO: usb_setupMSD(device);
            }
            else
            {
#if DEBUG
                DBGMSG("Unrecognised USB device detected.");
                BasicConsole.DelayOutput(4);
#endif
            }
        }
        public static byte SetDeviceAddress(USBDevice device, byte num)
        {
#if DEBUG
            DBGMSG("");
            DBGMSG("USB: SET_ADDRESS");
#endif

            byte new_address = num; // indicated port number

            USBTransfer transfer = new USBTransfer();
            device.hc.SetupTransfer(device, transfer, USBTransferType.USB_CONTROL, 0, 64);
            device.hc.SETUPTransaction(transfer, 8, 0x00, 5, 0, new_address, 0, 0);
            device.hc.INTransaction(transfer, true, null, 0);
            device.hc.IssueTransfer(transfer);

#if DEBUG
            DBGMSG(((FOS_System.String)"new address: ") + new_address);
            BasicConsole.DelayOutput(4);
#endif
            return new_address;
        }
        public static bool GetDeviceDescriptor(USBDevice device)
        {
#if DEBUG
            DBGMSG("USB: GET_DESCRIPTOR Device");
#endif

            USBDeviceDescriptor descriptor = new USBDeviceDescriptor();

            USBTransfer transfer = new USBTransfer();
            device.hc.SetupTransfer(device, transfer, USBTransferType.USB_CONTROL, 0, 64);
            device.hc.SETUPTransaction(transfer, 8, 0x80, 6, 1, 0, 0, 18);
            device.hc.INTransaction(transfer, false, &descriptor, 18);
            device.hc.OUTTransaction(transfer, true, null, 0);
            device.hc.IssueTransfer(transfer);

            if (transfer.success)
            {
                AnalyzeDeviceDescriptor(descriptor, device);
                ShowDevice(device);
                BasicConsole.DelayOutput(4);
            }

            return transfer.success;
        }
        private static void AnalyzeDeviceDescriptor(USBDeviceDescriptor d, USBDevice usbDev)
        {
            usbDev.usbSpec              = d.bcdUSB;
            usbDev.usbClass             = d.deviceClass;
            usbDev.usbSubclass          = d.deviceSubclass;
            usbDev.usbProtocol          = d.deviceProtocol;
            usbDev.vendor               = d.idVendor;
            usbDev.product              = d.idProduct;
            usbDev.releaseNumber        = d.bcdDevice;
            usbDev.manufacturerStringID = d.manufacturer;
            usbDev.productStringID      = d.product;
            usbDev.serNumberStringID    = d.serialNumber;
            usbDev.numConfigurations    = d.numConfigurations;
            ((USBEndpoint)usbDev.Endpoints[0]).mps     = d.maxPacketSize;
        }
        public static void ShowDevice(USBDevice usbDev)
        {
            if (usbDev.usbSpec == 0x0100 || usbDev.usbSpec == 0x0110 || usbDev.usbSpec == 0x0200 || usbDev.usbSpec == 0x0201 || usbDev.usbSpec == 0x0210 || usbDev.usbSpec == 0x0213 ||usbDev.usbSpec == 0x0300)
            {
                DBGMSG(((FOS_System.String)"USB ") + ((usbDev.usbSpec >> 8) & 0xFF) + "." + (usbDev.usbSpec & 0xFF)); // e.g. 0x0210 means 2.10
            }
            else
            {
                DBGMSG(((FOS_System.String)"Invalid USB version ") + ((usbDev.usbSpec >> 8) & 0xFF) + "." + (usbDev.usbSpec & 0xFF) + "!");
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
#if DEBUG
            DBGMSG(((FOS_System.String)"vendor:            ") + usbDev.vendor);
            DBGMSG(((FOS_System.String)"product:           ") + usbDev.product);
            DBGMSG(((FOS_System.String)"release number:    ") + ((usbDev.releaseNumber >> 8) & 0xFF) + "." + (usbDev.releaseNumber & 0xFF));
            DBGMSG(((FOS_System.String)"manufacturer:      ") + usbDev.manufacturerStringID);
            DBGMSG(((FOS_System.String)"product:           ") + usbDev.productStringID);
            DBGMSG(((FOS_System.String)"serial number:     ") + usbDev.serNumberStringID);
            DBGMSG(((FOS_System.String)"number of config.: ") + usbDev.numConfigurations); // number of possible configurations
            DBGMSG(((FOS_System.String)"MSDInterfaceNum:   ") + usbDev.MSDInterfaceNum);
            BasicConsole.DelayOutput(5);
#endif
        }
        public static unsafe bool GetConfigDescriptor(USBDevice device)
        {
#if DEBUG
            DBGMSG("USB: GET_DESCRIPTOR Config");
#endif

            //Pre-allocated 64-char empty string
            // "                                                                "
            FOS_System.String buffer = (FOS_System.String)"                                                                                                                                ";

            USBTransfer transfer = new USBTransfer();
            device.hc.SetupTransfer(device, transfer, USBTransferType.USB_CONTROL, 0, 64);
            device.hc.SETUPTransaction(transfer, 8, 0x80, 6, 2, 0, 0, 64);
            device.hc.INTransaction(transfer, false, buffer.GetCharPointer(), 64);
            device.hc.OUTTransaction(transfer, true, null, 0);
            device.hc.IssueTransfer(transfer);

            if (transfer.success)
            {
                // parse to config (len=9,type=2), interface (len=9,type=4) or endpoint (len=7,type=5)
#if DEBUG
                DBGMSG("---------------------------------------------------------------------");
                DBGMSG(buffer);
#endif
                byte* firstCharPtr = (byte*)buffer.GetCharPointer();
                byte* addr = firstCharPtr;
                byte* lastByte = addr + buffer.length; // totalLength (WORD)

                ushort numEndpoints = 1;
                // First pass. Retrieve usb_interfaceDescriptor which contains the number of endpoints
                while (addr < lastByte && addr < (firstCharPtr + 64))
                {
                    byte type = *(addr + 1);
                    byte length = *addr;

                    if (length == 9 && type == 2)
                    {
                        usb_configurationDescriptor* descriptor = (usb_configurationDescriptor*)addr;
                        ShowConfigurationDescriptor(descriptor);
                    }
                    else if (length == 9 && type == 4)
                    {
                        usb_interfaceDescriptor* descriptor = (usb_interfaceDescriptor*)addr;
                        ShowInterfaceDescriptor(descriptor);

                        if (descriptor->interfaceClass == 8)
                        {
                            // store interface number for mass storage transfers
                            device.MSDInterfaceNum = descriptor->interfaceNumber;
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
#if DEBUG
                        DBGMSG(((FOS_System.String)"length: ") + length + " type: " + type + " - unknown");
                        break;
#endif
                    }
                    addr += length;
                }

                //    usb_endpoint_t* newEPs = malloc(sizeof(usb_endpoint_t) * numEndpoints, 0, "usbDev->endpoints");
                //    memcpy(newEPs, device->endpoints, sizeof(usb_endpoint_t) * 1);
                //    memset(newEPs + 1, 0, sizeof(usb_endpoint_t) * (numEndpoints - 1));
                //    free(device->endpoints);
                //    device->endpoints = newEPs;

                //    // Second pass. Fill in endpoint information
                //    addr = buffer;
                //    lastByte = addr + (*(uint16_t*)(addr + 2));
                //    while ((uintptr_t)addr < (uintptr_t)lastByte && addr < (void*)(buffer + 64))
                //    {
                //        uint8_t type = *(uint8_t*)(addr + 1);
                //        uint8_t length = *(uint8_t*)addr;

                //        if (length == 7 && type == 5)
                //        {
                //            struct usb_endpointDescriptor* descriptor = addr;
                //            showEndpointDescriptor(descriptor);

                //            uint8_t ep_id = descriptor->endpointAddress & 0xF;
                //            ASSERT(ep_id < numEndpoints);

                //            device->endpoints[ep_id].mps = descriptor->maxPacketSize;
                //            device->endpoints[ep_id].type = EP_BIDIR; // Can be overwritten below
                //            device->endpoints[ep_id].interval = descriptor->interval;

                //            // store endpoint numbers for IN/OUT mass storage transfers, attributes must be 0x2, because there are also endpoints with attributes 0x3(interrupt)
                //            if (descriptor->endpointAddress & 0x80 && descriptor->attributes == 0x2)
                //            {
                //                if (ep_id < 3)
                //                    device->numEndpointInMSD = ep_id;
                //                device->endpoints[ep_id].type = EP_IN;
                //            }

                //            if (!(descriptor->endpointAddress & 0x80) && descriptor->attributes == 0x2)
                //            {
                //                if (ep_id < 3)
                //                    device->numEndpointOutMSD = ep_id;
                //                device->endpoints[ep_id].type = EP_OUT;
                //            }
                //        }

                //        addr += length;
                //    }//while
            }

            return false; // transfer.success;
        }
        private static void ShowConfigurationDescriptor(usb_configurationDescriptor* d)
        {
            if (d->length > 0)
            {
#if DEBUG
                DBGMSG(((FOS_System.String)"length:               ") + d->length);
                DBGMSG(((FOS_System.String)"descriptor type:      ") + d->descriptorType);
                DBGMSG(((FOS_System.String)"total length:         ") + d->totalLength);
#endif
                DBGMSG(((FOS_System.String)"Number of interfaces: ") + d->numInterfaces);
#if DEBUG
                DBGMSG(((FOS_System.String)"ID of config:         ") + d->configurationValue);
                DBGMSG(((FOS_System.String)"ID of config name     ") + d->configuration);
                DBGMSG(((FOS_System.String)"remote wakeup:        ") + (((d->attributes & Utilities.ConstantsUtils.BIT(5)) > 0) ? "yes" : "no"));
                DBGMSG(((FOS_System.String)"self-powered:         ") + (((d->attributes & Utilities.ConstantsUtils.BIT(6)) > 0) ? "yes" : "no"));
                DBGMSG(((FOS_System.String)"max power (mA):       ") + d->maxPower * 2); // 2 mA steps used
#endif
            }
        }
        private static void ShowInterfaceDescriptor(usb_interfaceDescriptor* d)
        {
            if (d->length > 0)
            {
#if DEBUG
                DBGMSG("---------------------------------------------------------------------");
                DBGMSG(((FOS_System.String)"length:               ") + d->length);          // 9
                DBGMSG(((FOS_System.String)"descriptor type:      ") + d->descriptorType);  // 4
#endif

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

#if DEBUG
                DBGMSG(((FOS_System.String)"alternate Setting:  ") + d->alternateSetting);
                DBGMSG(((FOS_System.String)"interface class:      ") + d->interfaceClass);
                DBGMSG(((FOS_System.String)"interface subclass:   ") + d->interfaceSubclass);
                DBGMSG(((FOS_System.String)"interface protocol:   ") + d->interfaceProtocol);
                DBGMSG(((FOS_System.String)"interface:            ") + d->Interface);
#endif
            }
        }
        public static void GetStringDescriptor(USBDevice device)
        {
        }
        public static void GetUnicodeStringDescriptor(USBDevice device, uint stringIndex)
        {
        }
        public static void SetConfiguration(USBDevice device, uint configuration)
        {
        }
        public static byte GetConfiguration(USBDevice device)
        {
            return 0;
        }
        public static ushort GetStatus(USBDevice device, uint endpoint)
        {
            return 0xDEAD;
        }
        public static void SetFeatureHALT(USBDevice device, uint endpoint)
        {
        }
        public static void ClearFeatureHALT(USBDevice device, uint endpoint)
        {
        }


#if DEBUG
        private static void DBGMSG(FOS_System.String msg)
        {
            BasicConsole.WriteLine(msg);
        }
#endif
    }
}
