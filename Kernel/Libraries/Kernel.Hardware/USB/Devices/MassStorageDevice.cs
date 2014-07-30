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

#define MSD_TRACE
#undef MSD_TRACE

#define DEVICE_INFO
#undef DEVICE_INFO

using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.Devices;
using Kernel.Utilities;
using Utils = Kernel.Utilities.MemoryUtils;

namespace Kernel.Hardware.USB.Devices
{
    public static class MassStorageDevice_Consts
    {
        public const uint CSWMagicNotOK = 0x01010101;
        public const uint CSWMagicOK = 0x53425355; // USBS
        public const uint CBWMagic = 0x43425355; // USBC
    }
    public unsafe class MassStorageDevice : USBDevice
    {
        protected MassStorageDevice_DiskDevice diskDevice;

        public MassStorageDevice(USBDeviceInfo aDeviceInfo)
            : base(aDeviceInfo)
        {
#if MSD_TRACE
            DBGMSG("------------------------------ Mass Storage Device -----------------------------");
            DBGMSG(((FOS_System.String)"MSD Interface num: ") + DeviceInfo.MSD_InterfaceNum);
            BasicConsole.DelayOutput(1);
#endif
            Setup();
        }

        protected void Setup()
        {
            // maxLUN (0 for USB-sticks)
            DeviceInfo.maxLUN = 0;

            // start with correct endpoint toggles and reset interface
            ((USBEndpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).toggle = false;
            ((USBEndpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_INEndpointID]).toggle = false;

            BulkReset(DeviceInfo.MSD_InterfaceNum); // Reset Interface

            byte* inquiryBuffer = (byte*)FOS_System.Heap.AllocZeroed(26u);
            SendSCSICommand(0x12 /*SCSI opcode*/, 0 /*LBA*/, 36 /*Bytes In*/, inquiryBuffer, null);

            AnalyzeInquiry(inquiryBuffer);

            ///////// send SCSI command "test unit ready(6)"
            TestDeviceReady();

            diskDevice = new MassStorageDevice_DiskDevice(this);
        }

        public override void Destroy()
        {
            diskDevice.Destroy();
            diskDevice = null;

            base.Destroy();
        }


        // Bulk-Only Mass Storage get maximum number of Logical Units
//        public byte GetMaxLUN(byte numInterface)
//        {
//#if MSD_TRACE
//            DBGMSG(((FOS_System.String)"USB MSD: TransferBulkOnly - GetMaxLUN, interface: ") + numInterface);
//#endif

//            byte maxLUN;

//            USBTransfer transfer = new USBTransfer();
//            DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.USB_CONTROL, 0, 64);

//            // bmRequestType bRequest  wValue wIndex    wLength   Data
//            // 10100001b     11111110b 0000h  Interface 0001h     1 byte
//            DeviceInfo.hc.SETUPTransaction(transfer, 8, 0xA1, 0xFE, 0, 0, numInterface, 1);
//            DeviceInfo.hc.INTransaction(transfer, false, &maxLUN, 1);
//            DeviceInfo.hc.OUTTransaction(transfer, true, null, 0); // handshake
//            DeviceInfo.hc.IssueTransfer(transfer);

//            return maxLUN;
//        }


        public void BulkReset()
        {
            BulkReset(DeviceInfo.MSD_InterfaceNum);
        }
        /// <summary>
        /// Bulk-Only Mass Storage Reset
        /// </summary>
        /// <param name="numInterface">Interface number to reset.</param> 
        public void BulkReset(byte numInterface)
        {
#if MSD_TRACE
            DBGMSG(((FOS_System.String)"USB MSD: TransferBulkOnly - MassStorageReset, interface: ") + numInterface);
#endif

            USBTransfer transfer = new USBTransfer();
            DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.USB_CONTROL, 0, 64);

            // bmRequestType bRequest  wValue wIndex    wLength   Data
            // 00100001b     11111111b 0000h  Interface 0000h     none
            DeviceInfo.hc.SETUPTransaction(transfer, 8, 0x21, 0xFF, 0, 0, numInterface, 0);
            DeviceInfo.hc.INTransaction(transfer, true, null, 0); // handshake
            DeviceInfo.hc.IssueTransfer(transfer);
        }

        public void FormatSCSICommand(byte SCSIcommand, usb_CommandBlockWrapper* cbw, uint LBA, ushort TransferLength)
        {
            cbw->CBWSignature = MassStorageDevice_Consts.CBWMagic;                      // magic
            cbw->CBWTag = 0x42424200u | SCSIcommand;      // device echoes this field in the CSWTag field of the associated CSW
            cbw->CBWDataTransferLength = TransferLength;        // Transfer length in bytes (only data)
            cbw->commandByte[0] = SCSIcommand;           // Operation code
            switch (SCSIcommand)
            {
                case 0x00: // test unit ready(6)
                    cbw->CBWFlags = 0x00;          // Out: 0x00  In: 0x80
                    cbw->CBWCBLength = 6;             // only bits 4:0
                    break;
                case 0x03: // Request Sense(6)
                    cbw->CBWFlags = 0x80;          // Out: 0x00  In: 0x80
                    cbw->CBWCBLength = 6;             // only bits 4:0
                    cbw->commandByte[4] = 18;            // Allocation length (max. bytes)
                    break;
                case 0x12: // Inquiry(6)
                    cbw->CBWFlags = 0x80;          // Out: 0x00  In: 0x80
                    cbw->CBWCBLength = 6;             // only bits 4:0
                    cbw->commandByte[4] = 36;            // Allocation length (max. bytes)
                    break;
                case 0x25: // read capacity(10)
                    cbw->CBWFlags = 0x80;          // Out: 0x00  In: 0x80
                    cbw->CBWCBLength = 10;            // only bits 4:0
                    cbw->commandByte[2] = (byte)(LBA >> 24);    // LBA MSB
                    cbw->commandByte[3] = (byte)(LBA >> 16);    // LBA
                    cbw->commandByte[4] = (byte)(LBA >> 8);    // LBA
                    cbw->commandByte[5] = (byte)(LBA);    // LBA LSB
                    break;
                case 0x28: // read(10)
                    cbw->CBWFlags = 0x80;                  // Out: 0x00  In: 0x80
                    cbw->CBWCBLength = 10;                    // only bits 4:0
                    cbw->commandByte[2] = (byte)(LBA >> 24);            // LBA MSB
                    cbw->commandByte[3] = (byte)(LBA >> 16);            // LBA
                    cbw->commandByte[4] = (byte)(LBA >> 8);            // LBA
                    cbw->commandByte[5] = (byte)(LBA);            // LBA LSB
                    cbw->commandByte[7] = (byte)((TransferLength / (uint)diskDevice.BlockSize) >> 8); // MSB <--- blocks not byte!
                    cbw->commandByte[8] = (byte)(TransferLength / (uint)diskDevice.BlockSize); // LSB
                    break;
                case 0x2A: // write(10)
                    cbw->CBWFlags = 0x00;                  // Out: 0x00  In: 0x80
                    cbw->CBWCBLength = 10;                    // only bits 4:0
                    cbw->commandByte[2] = (byte)(LBA >> 24);            // LBA MSB
                    cbw->commandByte[3] = (byte)(LBA >> 16);            // LBA
                    cbw->commandByte[4] = (byte)(LBA >> 8);            // LBA
                    cbw->commandByte[5] = (byte)(LBA);            // LBA LSB
                    cbw->commandByte[7] = (byte)((TransferLength / (uint)diskDevice.BlockSize) >> 8); // MSB <--- blocks not byte!
                    cbw->commandByte[8] = (byte)(TransferLength / (uint)diskDevice.BlockSize); // LSB
                    break;
            }
        }
        public int CheckSCSICommand(void* MSDStatus, ushort TransferLength, byte SCSIOpcode)
        {
            int error = 0;

            // check signature 0x53425355 // DWORD 0 (byte 0:3)
            uint CSWsignature = *(uint*)MSDStatus; // DWORD 0
            if (CSWsignature == MassStorageDevice_Consts.CSWMagicOK)
            {
#if MSD_TRACE
                DBGMSG("CSW signature OK");
#endif
            }
            else if (CSWsignature == MassStorageDevice_Consts.CSWMagicNotOK)
            {
#if MSD_TRACE
                DBGMSG("CSW signature wrong (not processed)");
#endif
                return -1;
            }
            else
            {
#if MSD_TRACE
                DBGMSG("CSW signature wrong (processed, but wrong value)");
#endif
                error = -2;
            }

            // check matching tag
            uint CSWtag = *(((uint*)MSDStatus) + 1); // DWORD 1 (byte 4:7)

            if (((byte)(CSWtag) == SCSIOpcode) && ((byte)(CSWtag >> 8) == 0x42) && ((byte)(CSWtag >> 16) == 0x42) && ((byte)(CSWtag >> 24) == 0x42))
            {
#if MSD_TRACE
                DBGMSG(((FOS_System.String)"CSW tag ") + (byte)(CSWtag) + " OK");
#endif
            }
            else
            {
#if MSD_TRACE
                DBGMSG("Error: CSW tag wrong");
#endif
                error = -3;
            }

            // check CSWDataResidue
            uint CSWDataResidue = *(((uint*)MSDStatus) + 2); // DWORD 2 (byte 8:11)
            if (CSWDataResidue == 0)
            {
#if MSD_TRACE
                DBGMSG("CSW data residue OK");
#endif
            }
            else
            {
#if MSD_TRACE
                DBGMSG(((FOS_System.String)"CSW data residue: ") + CSWDataResidue);
#endif
            }

            // check status byte // DWORD 3 (byte 12)
            byte CSWstatusByte = *(((byte*)MSDStatus) + 12); // byte 12 (last byte of 13 bytes)

            switch (CSWstatusByte)
            {
                case 0x00:
#if MSD_TRACE
                    DBGMSG("CSW status OK");
#endif
                    break;
                case 0x01:
#if MSD_TRACE
                    DBGMSG("Command failed");
#endif
                    return -4;
                case 0x02:
#if MSD_TRACE
                    DBGMSG("Phase Error");
                    DBGMSG("Reset recovery is needed");
#endif
                    ResetRecoveryMSD(DeviceInfo.MSD_InterfaceNum);
                    return -5;
                default:
#if MSD_TRACE
                    DBGMSG("CSW status byte: undefined value (error)");
#endif
                    return -6;
            }

            return error;
        }

        // cf. http://www.beyondlogic.org/usbnutshell/usb4.htm#Bulk
        public void SendSCSICommand(byte SCSIcommand, uint LBA, ushort TransferLength, void* dataBuffer, void* statusBuffer)
        {
#if MSD_TRACE
            DBGMSG("OUT part");
            DBGMSG(((FOS_System.String)"Toggle OUT ") + ((USBEndpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).toggle);
#endif

            usb_CommandBlockWrapper* cbw = (usb_CommandBlockWrapper*)FOS_System.Heap.AllocZeroed((uint)sizeof(usb_CommandBlockWrapper));
            bool FreeStatusBuffer = false;
            try
            {
                FormatSCSICommand(SCSIcommand, cbw, LBA, TransferLength);

                USBTransfer transfer = new USBTransfer();
                DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.USB_BULK, DeviceInfo.MSD_OUTEndpointID, 512);
                DeviceInfo.hc.OUTTransaction(transfer, false, cbw, 31);
                DeviceInfo.hc.IssueTransfer(transfer);

                /**************************************************************************************************************************************/

                if (transfer.success)
                {
#if MSD_TRACE
                    DBGMSG("IN part");
#endif

                    if (statusBuffer == null)
                    {
#if MSD_TRACE
                        DBGMSG("Alloc 13 bytes of mem...");
#endif
                        FreeStatusBuffer = true;
                        statusBuffer = FOS_System.Heap.AllocZeroed(13u);
                    }

#if MSD_TRACE
                    DBGMSG("Setup transfer...");
#endif
                    DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.USB_BULK, DeviceInfo.MSD_INEndpointID, 512);
#if MSD_TRACE
                    DBGMSG("Done.");
#endif
                    if (TransferLength > 0)
                    {
#if MSD_TRACE
                        DBGMSG("Setup IN transactions...");
#endif
                        DeviceInfo.hc.INTransaction(transfer, false, dataBuffer, TransferLength);
                        DeviceInfo.hc.INTransaction(transfer, false, statusBuffer, 13);
#if MSD_TRACE
                        DBGMSG("Done.");
#endif
                    }
                    else
                    {
                        DeviceInfo.hc.INTransaction(transfer, false, statusBuffer, 13);
                    }
#if MSD_TRACE
                    DBGMSG("Issue transfer...");
#endif
                    DeviceInfo.hc.IssueTransfer(transfer);
#if MSD_TRACE
                    DBGMSG("Done.");
                    DBGMSG("Check command...");
#endif

                    if (!transfer.success || CheckSCSICommand(statusBuffer, TransferLength, SCSIcommand) != 0)
                    {
                        // TODO: Handle failure/timeout
#if MSD_TRACE
                        DBGMSG("SCSI IN command failed!");
#endif
                    }
#if MSD_TRACE
                    else
                    {
                        DBGMSG("Command OK.");
                        BasicConsole.DelayOutput(1);
                    }
#endif
                }
                else
                {
                    // TODO: Handle failure/timeout
#if MSD_TRACE
                    DBGMSG("SCSI OUT command failed!");
#endif
                }
            }
            finally
            {
                FOS_System.Heap.Free(cbw);
                if (FreeStatusBuffer)
                {
                    FOS_System.Heap.Free(statusBuffer);
                }
            }
        }
        public void SendSCSICommand_OUT(byte SCSIcommand, uint LBA, ushort TransferLength, void* dataBuffer, void* statusBuffer)
        {
            usb_CommandBlockWrapper* cbw = (usb_CommandBlockWrapper*)FOS_System.Heap.AllocZeroed((uint)sizeof(usb_CommandBlockWrapper));
            bool FreeStatusBuffer = false;
            try
            {
                FormatSCSICommand(SCSIcommand, cbw, LBA, TransferLength);

                USBTransfer transfer = new USBTransfer();
                DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.USB_BULK, DeviceInfo.MSD_OUTEndpointID, 512);
                DeviceInfo.hc.OUTTransaction(transfer, false, cbw, 31);
                DeviceInfo.hc.OUTTransaction(transfer, false, dataBuffer, TransferLength);
                DeviceInfo.hc.IssueTransfer(transfer);

                /**************************************************************************************************************************************/

                if (statusBuffer == null)
                {
                    statusBuffer = FOS_System.Heap.AllocZeroed(13u);
                }

                DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.USB_BULK, DeviceInfo.MSD_INEndpointID, 512);
                DeviceInfo.hc.INTransaction(transfer, false, statusBuffer, 13);
                DeviceInfo.hc.IssueTransfer(transfer);
            }
            finally
            {
                FOS_System.Heap.Free(cbw);
                if (FreeStatusBuffer)
                {
                    FOS_System.Heap.Free(statusBuffer);
                }
            }
        }
        
        public byte TestDeviceReady()
        {
            byte maxTest = 5;
            uint timeout = maxTest;
            byte statusByte = 0;

            while (timeout != 0)
            {
                timeout--;
#if MSD_TRACE
                DBGMSG("SCSI: test unit ready");
#endif

                byte* statusBuffer = (byte*)FOS_System.Heap.AllocZeroed(13u);
                SendSCSICommand(0x00, 0u, 0, null, statusBuffer); // dev, endp, cmd, LBA, transfer length

                byte statusByteTestReady = (byte)*(((uint*)statusBuffer) + 3);

                if (timeout >= maxTest / 2 && statusByteTestReady != 0) continue;

#if MSD_TRACE
                DBGMSG("SCSI: request sense");
#endif

                byte* dataBuffer = (byte*)FOS_System.Heap.AllocZeroed(18u);
                SendSCSICommand(0x03, 0, 18, dataBuffer, statusBuffer); // dev, endp, cmd, LBA, transfer length

                statusByte = (byte)*(((uint*)statusBuffer) + 3);

                int sense = ShowResultsRequestSense(dataBuffer);
                if (sense == 0 || sense == 6)
                {
                    break;
                }

            }

            return statusByte;
        }
        // http://en.wikipedia.org/wiki/SCSI_Inquiry_Command
        static void AnalyzeInquiry(byte* addr)
        {
            // cf. Jan Axelson, USB Mass Storage, page 140
            byte PeripheralDeviceType = Utils.GetField(addr, 0, 0, 5); // byte, shift, len
            // uint8_t PeripheralQualifier  = getField(addr, 0, 5, 3);
            // uint8_t DeviceTypeModifier   = getField(addr, 1, 0, 7);
            byte RMB = Utils.GetField(addr, 1, 7, 1);
#if MSD_TRACE
            byte ANSIapprovedVersion = Utils.GetField(addr, 2, 0, 3);
#endif
            // uint8_t ECMAversion          = getField(addr, 2, 3, 3);
            // uint8_t ISOversion           = getField(addr, 2, 6, 2);
            byte ResponseDataFormat = Utils.GetField(addr, 3, 0, 4);
            byte HISUP = Utils.GetField(addr, 3, 4, 1);
            byte NORMACA = Utils.GetField(addr, 3, 5, 1);
            // uint8_t AdditionalLength     = getField(addr, 4, 0, 8);
            byte CmdQue = Utils.GetField(addr, 7, 1, 1);
            byte Linked = Utils.GetField(addr, 7, 3, 1);

#if MSD_TRACE || DEVICE_INFO
            BasicConsole.WriteLine("Vendor ID  : " + FOS_System.ByteConverter.GetASCIIStringFromASCII(addr, 8, 8));
            BasicConsole.WriteLine("Product ID : " + FOS_System.ByteConverter.GetASCIIStringFromASCII(addr, 16, 16));

            DBGMSG("Revision   : " + FOS_System.ByteConverter.GetASCIIStringFromASCII(addr, 32, 4));

            // Book of Jan Axelson, "USB Mass Storage", page 140:
            // printf("\nVersion ANSI: %u  ECMA: %u  ISO: %u", ANSIapprovedVersion, ECMAversion, ISOversion);
            DBGMSG(((FOS_System.String)"Version: ") + ANSIapprovedVersion + " (4: SPC-2, 5: SPC-3)");

            // Jan Axelson, USB Mass Storage, page 140
            if (ResponseDataFormat == 2)
            {
                BasicConsole.WriteLine("Response Data Format OK");
            }
            else
            {
                BasicConsole.WriteLine(((FOS_System.String)"Response Data Format is not OK: ") + ResponseDataFormat + " (should be 2)");
            }

            BasicConsole.WriteLine(((FOS_System.String)"Removable device type:            ") + (RMB != 0 ? "yes" : "no"));
            BasicConsole.WriteLine(((FOS_System.String)"Supports hierarch. addr. support: ") + (HISUP != 0 ? "yes" : "no"));
            BasicConsole.WriteLine(((FOS_System.String)"Supports normal ACA bit support:  ") + (NORMACA != 0 ? "yes" : "no"));
            BasicConsole.WriteLine(((FOS_System.String)"Supports linked commands:         ") + (Linked != 0 ? "yes" : "no"));
            BasicConsole.WriteLine(((FOS_System.String)"Supports tagged command queuing:  ") + (CmdQue != 0 ? "yes" : "no"));

            switch (PeripheralDeviceType)
            {
                case 0x00: BasicConsole.WriteLine("direct-access device (e.g., magnetic disk)"); break;
                case 0x01: BasicConsole.WriteLine("sequential-access device (e.g., magnetic tape)"); break;
                case 0x02: BasicConsole.WriteLine("printer device"); break;
                case 0x03: BasicConsole.WriteLine("processor device"); break;
                case 0x04: BasicConsole.WriteLine("write-once device"); break;
                case 0x05: BasicConsole.WriteLine("CD/DVD device"); break;
                case 0x06: BasicConsole.WriteLine("scanner device"); break;
                case 0x07: BasicConsole.WriteLine("optical memory device (non-CD optical disk)"); break;
                case 0x08: BasicConsole.WriteLine("medium Changer (e.g. jukeboxes)"); break;
                case 0x09: BasicConsole.WriteLine("communications device"); break;
                case 0x0A: BasicConsole.WriteLine("defined by ASC IT8 (Graphic arts pre-press devices)"); break;
                case 0x0B: BasicConsole.WriteLine("defined by ASC IT8 (Graphic arts pre-press devices)"); break;
                case 0x0C: BasicConsole.WriteLine("Storage array controller device (e.g., RAID)"); break;
                case 0x0D: BasicConsole.WriteLine("Enclosure services device"); break;
                case 0x0E: BasicConsole.WriteLine("Simplified direct-access device (e.g., magnetic disk)"); break;
                case 0x0F: BasicConsole.WriteLine("Optical card reader/writer device"); break;
                case 0x10: BasicConsole.WriteLine("Reserved for bridging expanders"); break;
                case 0x11: BasicConsole.WriteLine("Object-based Storage Device"); break;
                case 0x12: BasicConsole.WriteLine("Automation/Drive Interface"); break;
                case 0x13:
                case 0x1D: BasicConsole.WriteLine("Reserved"); break;
                case 0x1E: BasicConsole.WriteLine("Reduced block command (RBC) direct-access device"); break;
                case 0x1F: BasicConsole.WriteLine("Unknown or no device type"); break;
            }
#endif
        }

        public void TestMSD()
        {
            // maxLUN (0 for USB-sticks)
            DeviceInfo.maxLUN = 0;

            // start with correct endpoint toggles and reset interface
            ((USBEndpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).toggle = false;
            ((USBEndpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_INEndpointID]).toggle = false;

            BulkReset(DeviceInfo.MSD_InterfaceNum); // Reset Interface

            ///////// send SCSI command "inquiry (opcode: 0x12)"
#if MSD_TRACE
            DBGMSG("SCSI: inquiry");
#endif
              
#if DEVICE_INFO || MSD_TRACE
            BasicConsole.DelayOutput(10);
#endif

            ///////// send SCSI command "test unit ready(6)"
            TestDeviceReady();

            ///////// send SCSI command "read capacity(10)"
#if MSD_TRACE
            DBGMSG("SCSI: read capacity");
#endif

            uint* capacityBuffer = (uint*)FOS_System.Heap.AllocZeroed(8);
            SendSCSICommand(0x25 /*SCSI opcode*/, 0 /*LBA*/, 8 /*Bytes In*/, capacityBuffer, null);

            // MSB ... LSB
            capacityBuffer[0] = Utils.htonl(capacityBuffer[0]);
            capacityBuffer[1] = Utils.htonl(capacityBuffer[1]);

            //diskDevice.SetBlockCount(((ulong)capacityBuffer[0]) + 1);
            //diskDevice.SetBlockSize((ulong)capacityBuffer[1]);
            
#if MSD_TRACE
            DBGMSG(((FOS_System.String)"Capacity: ") + (diskDevice.BlockCount * diskDevice.BlockSize) + ", Last LBA: " + capacityBuffer[0] + ", block size: " + capacityBuffer[1]);
#endif
        }

        public bool Read(uint sector, void* buffer)
        {
#if MSD_TRACE
            DBGMSG(((FOS_System.String)">SCSI: read sector: ") + sector);
#endif

            SendSCSICommand(0x28 /*SCSI opcode*/, sector /*LBA*/, (ushort)diskDevice.BlockSize /*Bytes In*/, buffer, null);

            return true;
        }
        public bool Write(uint sector, void* buffer)
        {
#if MSD_TRACE
            DBGMSG(((FOS_System.String)">SCSI: write sector: ") + sector);
#endif

            SendSCSICommand_OUT(0x2A /*SCSI opcode*/, sector /*LBA*/, (ushort)diskDevice.BlockSize /*Bytes Out*/, buffer, null);

            return true;
        }

        public void ResetRecoveryMSD(byte Interface)
        {
            // Reset Interface
            BulkReset(Interface);

            // TEST ////////////////////////////////////
            //usbSetFeatureHALT(device, device->numEndpointInMSD);
            //usbSetFeatureHALT(device, device->numEndpointOutMSD);

            // Clear Feature HALT to the Bulk-In  endpoint
#if MSD_TRACE
            DBGMSG(((FOS_System.String)"GetStatus: ") + USBManager.GetStatus(DeviceInfo, DeviceInfo.MSD_INEndpointID));
#endif
            USBManager.ClearFeatureHALT(DeviceInfo, DeviceInfo.MSD_INEndpointID);
#if MSD_TRACE
            DBGMSG(((FOS_System.String)"GetStatus: ") + USBManager.GetStatus(DeviceInfo, DeviceInfo.MSD_INEndpointID));
#endif

            // Clear Feature HALT to the Bulk-Out endpoint
#if MSD_TRACE
            DBGMSG(((FOS_System.String)"GetStatus: ") + USBManager.GetStatus(DeviceInfo, DeviceInfo.MSD_OUTEndpointID));
#endif
            USBManager.ClearFeatureHALT(DeviceInfo, DeviceInfo.MSD_OUTEndpointID);
#if MSD_TRACE
            DBGMSG(((FOS_System.String)"GetStatus: ") + USBManager.GetStatus(DeviceInfo, DeviceInfo.MSD_OUTEndpointID));
#endif

            // set configuration to 1 and endpoint IN/OUT toggles to 0
            USBManager.SetConfiguration(DeviceInfo, 1); // set first configuration
            byte config = USBManager.GetConfiguration(DeviceInfo);
#if MSD_TRACE
            if (config != 1)
            {
                DBGMSG(((FOS_System.String)"Configuration: ") + config + " (to be: 1)");
            }
#endif

            // start with correct endpoint toggles and reset interface
            ((USBEndpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).toggle = false;
            ((USBEndpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_INEndpointID]).toggle = false;
            BulkReset(DeviceInfo.MSD_InterfaceNum); // Reset Interface
        }

        public static FOS_System.String[] SenseKeys =
        {
            "No Sense",
            "Recovered Error - last command completed with some recovery action",
            "Not Ready - logical unit addressed cannot be accessed",
            "Medium Error - command terminated with a non-recovered error condition",
            "Hardware Error",
            "Illegal Request - illegal parameter in the command descriptor block",
            "Unit Attention - disc drive may have been reset.",
            "Data Protect - command read/write on a protected block",
            "not defined",
            "Firmware Error",
            "not defined",
            "Aborted Command - disc drive aborted the command",
            "Equal - SEARCH DATA command has satisfied an equal comparison",
            "Volume Overflow - buffered peripheral device has reached the end of medium partition",
            "Miscompare - source data did not match the data read from the medium",
            "not defined"
        };
        public static int ShowResultsRequestSense(byte* addr)
        {
            uint Valid = Utils.GetField(addr, 0, 7, 1); // byte 0, bit 7
            uint ResponseCode = Utils.GetField(addr, 0, 0, 7); // byte 0, bit 6:0
            uint SenseKey = Utils.GetField(addr, 2, 0, 4); // byte 2, bit 3:0
            
#if MSD_TRACE
            DBGMSG("Results of \"request sense\":");
#endif
            if (ResponseCode >= 0x70 && ResponseCode <= 0x73)
            {
#if MSD_TRACE
                DBGMSG("Valid:");
                if (Valid == 0)
                {
                    DBGMSG("Sense data are not SCSI compliant");
                }
                else
                {
                    DBGMSG("Sense data are SCSI compliant");
                }
                DBGMSG("Response Code:");
                switch (ResponseCode)
                {
                    case 0x70:
                        DBGMSG("Current errors, fixed format");
                        break;
                    case 0x71:
                        DBGMSG("Deferred errors, fixed format");
                        break;
                    case 0x72:
                        DBGMSG("Current error, descriptor format");
                        break;
                    case 0x73:
                        DBGMSG("Deferred error, descriptor format");
                        break;
                    default:
                        DBGMSG("No valid response code!");
                        break;
                }

                DBGMSG("Sense Key:");
                if (SenseKey < SenseKeys.Length)
                    DBGMSG(SenseKeys[SenseKey]);
                else
                    DBGMSG("sense key not known!");
#endif
                return (int)SenseKey;
            }
            
#if MSD_TRACE
            DBGMSG("No vaild response code!");
#endif
            return -1;
        }

#if MSD_TRACE
        private static void DBGMSG(FOS_System.String msg)
        {
            BasicConsole.WriteLine(msg);
        }
#endif
    }
    public unsafe class MassStorageDevice_DiskDevice : DiskDevice
    {
        MassStorageDevice msd;

        public MassStorageDevice_DiskDevice(MassStorageDevice anMSD)
        {
            msd = anMSD;
            
            uint* capacityBuffer = (uint*)FOS_System.Heap.AllocZeroed(8);
            anMSD.SendSCSICommand(0x25 /*SCSI opcode*/, 0 /*LBA*/, 8 /*Bytes In*/, capacityBuffer, null);

            // MSB ... LSB
            capacityBuffer[0] = Utils.htonl(capacityBuffer[0]);
            capacityBuffer[1] = Utils.htonl(capacityBuffer[1]);

            blockCount = ((ulong)capacityBuffer[0]) + 1;
            blockSize = (ulong)capacityBuffer[1];

            DeviceManager.Devices.Add(this);
        }

        public override void ReadBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
#if MSD_TRACE
            BasicConsole.WriteLine("Beginning reading...");
#endif

            byte* dataPtr = ((byte*)Utilities.ObjectUtilities.GetHandle(aData)) + FOS_System.Array.FieldsBytesSize;
            for (uint i = 0; i < aBlockCount; i++)
            {
#if MSD_TRACE
                BasicConsole.Write(((FOS_System.String)"Reading block: ") + i);
#endif

                msd.Read((uint)(aBlockNo + i), dataPtr);
                dataPtr += blockSize;
                
#if MSD_TRACE
                BasicConsole.WriteLine(" - Read.");
#endif

                FOS_System.GC.Cleanup();
            }

#if MSD_TRACE
            BasicConsole.WriteLine("Completed all reading.");
#endif
        }
        public override void WriteBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            byte* dataPtr = ((byte*)Utilities.ObjectUtilities.GetHandle(aData)) + FOS_System.Array.FieldsBytesSize;
            for (uint i = 0; i < aBlockCount; i++)
            {
                msd.Write((uint)(aBlockNo + i), dataPtr);
                dataPtr += blockSize;

                FOS_System.GC.Cleanup();
            }
        }
        
        public void Destroy()
        {
            DeviceManager.Devices.Remove(this);
            msd = null;
        }
    }

    public unsafe struct usb_CommandBlockWrapper
    {
        public uint CBWSignature;
        public uint CBWTag;
        public uint CBWDataTransferLength;
        public byte CBWFlags;
        public byte CBWLUN;           // only bits 3:0
        public byte CBWCBLength;      // only bits 4:0
        public fixed byte commandByte[16];
    }
}
