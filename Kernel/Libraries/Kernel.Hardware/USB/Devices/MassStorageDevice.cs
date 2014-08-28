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
    /// <summary>
    /// Constant values used by the Mass Storage Device driver.
    /// </summary>
    public static class MassStorageDevice_Consts
    {
        /// <summary>
        /// Indicates the SCSI command is not OK. 
        /// </summary>
        public const uint CSWMagicNotOK = 0x01010101;
        /// <summary>
        /// Indiciates the SCSI command is OK.
        /// </summary>
        public const uint CSWMagicOK = 0x53425355; // USBS
        /// <summary>
        /// SCSI command packet siganture.
        /// </summary>
        public const uint CBWMagic = 0x43425355; // USBC
    }
    /// <summary>
    /// Represents a USB mass storage device.
    /// </summary>
    public unsafe class MassStorageDevice : USBDevice
    {
        //TODO - Uncomment try-finally once try-catch-finally works properly.
        //TODO - Test for ejected, don't send commands if ejected!

        /// <summary>
        /// The underlying disk device that allows the mass storage device to be 
        /// accessed in the same way as a disk.
        /// </summary>
        public MassStorageDevice_DiskDevice diskDevice
        {
            get;
            protected set;
        }
       
        public bool Ejected
        {
            get;
            protected set;
        }

        /// <summary>
        /// Initialises a new mass storage device with the specified USB device information.
        /// </summary>
        /// <param name="aDeviceInfo">
        /// The USB device information that specifies the physical mass storage device.
        /// </param>
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

        /// <summary>
        /// Sets up the mass storage device for bulk transfer. Also creates the mass storage disk device.
        /// </summary>
        protected void Setup()
        {
            // start with correct endpoint toggles and reset interface
            ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).toggle = false;
            ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_INEndpointID]).toggle = false;

            BulkReset(DeviceInfo.MSD_InterfaceNum); // Reset Interface

            byte* inquiryBuffer = (byte*)FOS_System.Heap.AllocZeroed(26u);
            //try
            {
                SendSCSICommand_IN(0x12 /*SCSI opcode*/, 0 /*LBA*/, 36 /*Bytes In*/, inquiryBuffer, null);

                AnalyseInquiry(inquiryBuffer);
            }
            //finally
            {
                FOS_System.Heap.Free(inquiryBuffer);
            }

            ///////// send SCSI command "test unit ready(6)"
            TestDeviceReady();

            diskDevice = new MassStorageDevice_DiskDevice(this);
        }

        /// <summary>
        /// Destroys the USB device.
        /// </summary>
        public override void Destroy()
        {
            Eject();

            diskDevice.Destroy();
            diskDevice = null;

            base.Destroy();
        }

        public void Eject()
        {
            if(!Ejected)
            {
                Ejected = true;

                //Synchronise Cache (10) Command
                SendSCSI_SyncCacheCommand(false, false, null);
            }
        }

        /// <summary>
        /// Resets the bulk tarnsfer interface.
        /// </summary>
        public void BulkReset()
        {
            BulkReset(DeviceInfo.MSD_InterfaceNum);
        }
        /// <summary>
        /// Resets the bulk tarnsfer interface.
        /// </summary>
        /// <param name="numInterface">The bulk transfer interface number to reset.</param> 
        public void BulkReset(byte numInterface)
        {
#if MSD_TRACE
            DBGMSG(((FOS_System.String)"USB MSD: TransferBulkOnly - MassStorageReset, interface: ") + numInterface);
#endif

            USBTransfer transfer = new USBTransfer();
            DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Control, 0, 64);

            // bmRequestType bRequest  wValue wIndex    wLength   Data
            // 00100001b     11111111b 0000h  Interface 0000h     none
            DeviceInfo.hc.SETUPTransaction(transfer, 8, 0x21, 0xFF, 0, 0, numInterface, 0);
            DeviceInfo.hc.INTransaction(transfer, true, null, 0); // handshake
            DeviceInfo.hc.IssueTransfer(transfer);
        }

        /// <summary>
        /// Sets up a SCSI command.
        /// </summary>
        /// <param name="SCSIcommand">The command byte.</param>
        /// <param name="cbw">A pointer to an existing command block wrapper.</param>
        /// <param name="LBA">The LBA to access.</param>
        /// <param name="TransferLength">The length of the data in bytes.</param>
        public void SetupSCSICommand(byte SCSIcommand, CommandBlockWrapper* cbw, uint LBA, ushort TransferLength)
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
                case 0x35: // SynchroniseCache(10)
                    cbw->CBWFlags = 0x00;
                    cbw->CBWCBLength = 10;
                    cbw->CBWDataTransferLength = 0;
                    cbw->commandByte[2] = (byte)(LBA >> 24);            // LBA MSB
                    cbw->commandByte[3] = (byte)(LBA >> 16);            // LBA
                    cbw->commandByte[4] = (byte)(LBA >> 8);            // LBA
                    cbw->commandByte[5] = (byte)(LBA);            // LBA LSB
                    cbw->commandByte[7] = (byte)((TransferLength / (uint)diskDevice.BlockSize) >> 8); // MSB <--- blocks not byte!
                    cbw->commandByte[8] = (byte)(TransferLength / (uint)diskDevice.BlockSize); // LSB
                    break;
            }
        }
        /// <summary>
        /// Checks a received SCSI command is valid and OK.
        /// </summary>
        /// <param name="MSDStatus">A pointer to the start of the SCSI command data.</param>
        /// <param name="SCSIOpcode">The SCSI op coce.</param>
        /// <returns>True if the command is completely valid. Otherwise, false.</returns>
        public int CheckSCSICommand(void* MSDStatus, byte SCSIOpcode)
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

        /// <summary>
        /// Sends a SCSI command that receives data.
        /// </summary>
        /// <param name="SCSIcommand">The SCSI command to send.</param>
        /// <param name="LBA">The LBA to access.</param>
        /// <param name="TransferLength">The length of the data to receive.</param>
        /// <param name="dataBuffer">The data buffer - must be at least as big as the transfer length.</param>
        /// <param name="statusBuffer">The buffer to store the command status result in. Must be at least 13 bytes long.</param>
        /// <see cref="!:http://www.beyondlogic.org/usbnutshell/usb4.htm#Bulk"/>
        public void SendSCSICommand_IN(byte SCSIcommand, uint LBA, ushort TransferLength, void* dataBuffer, void* statusBuffer)
        {
#if MSD_TRACE
            DBGMSG("OUT part");
            DBGMSG(((FOS_System.String)"Toggle OUT ") + ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).toggle);
#endif

            CommandBlockWrapper* cbw = (CommandBlockWrapper*)FOS_System.Heap.AllocZeroed((uint)sizeof(CommandBlockWrapper));
            bool FreeStatusBuffer = false;
            //try
            {
                SetupSCSICommand(SCSIcommand, cbw, LBA, TransferLength);

#if MSD_TRACE
                DBGMSG("Setup command. Transferring data...");
#endif
                USBTransfer transfer = new USBTransfer();
                DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_OUTEndpointID, 512);
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
                    DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_INEndpointID, 512);
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

                    if (!transfer.success || CheckSCSICommand(statusBuffer, SCSIcommand) != 0)
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
            //finally
            {
                FOS_System.Heap.Free(cbw);
                if (FreeStatusBuffer)
                {
                    FOS_System.Heap.Free(statusBuffer);
                }
            }
        }
        /// <summary>
        /// Sends a SCSI command that sends data.
        /// </summary>
        /// <param name="SCSIcommand">The SCSI command to send.</param>
        /// <param name="LBA">The LBA to access.</param>
        /// <param name="TransferLength">The length of the data to send.</param>
        /// <param name="dataBuffer">The data buffer - must be at least as long as the transfer length.</param>
        /// <param name="statusBuffer">The data buffer to store the command status result in. Must be at least 13 bytes long.</param>
        public void SendSCSICommand_OUT(byte SCSIcommand, uint LBA, ushort TransferLength, void* dataBuffer, void* statusBuffer)
        {
#if MSD_TRACE
            DBGMSG("OUT part");
            DBGMSG(((FOS_System.String)"Toggle OUT ") + ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).toggle);
#endif

            CommandBlockWrapper* cbw = (CommandBlockWrapper*)FOS_System.Heap.AllocZeroed((uint)sizeof(CommandBlockWrapper));
            bool FreeStatusBuffer = false;
            //try
            {
                SetupSCSICommand(SCSIcommand, cbw, LBA, TransferLength);

#if MSD_TRACE
                DBGMSG("Setup transfer...");
#endif
                USBTransfer transfer = new USBTransfer();
                DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_OUTEndpointID, 512);
                DeviceInfo.hc.OUTTransaction(transfer, false, cbw, 31);
                DeviceInfo.hc.IssueTransfer(transfer);

                if (transfer.success)
                {
                    DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_OUTEndpointID, 512);
                    DeviceInfo.hc.OUTTransaction(transfer, false, dataBuffer, TransferLength);
                    DeviceInfo.hc.IssueTransfer(transfer);

#if MSD_TRACE
                    if (!transfer.success)
                    {
                        DBGMSG("Transfer 2 failed!");
                        BasicConsole.DelayOutput(5);
                    }
#endif
                }
#if MSD_TRACE
                else
                {
                    DBGMSG("Transfer 1 failed!");
                    BasicConsole.DelayOutput(5);
                }
#endif

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
                    DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_INEndpointID, 512);
#if MSD_TRACE
                    DBGMSG("Done.");
#endif
                    DeviceInfo.hc.INTransaction(transfer, false, statusBuffer, 13);
#if MSD_TRACE
                    DBGMSG("Issue transfer...");
#endif
                    DeviceInfo.hc.IssueTransfer(transfer);
#if MSD_TRACE
                    DBGMSG("Done.");
                    DBGMSG("Check command...");
#endif

                    if (!transfer.success || CheckSCSICommand(statusBuffer, SCSIcommand) != 0)
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
            //finally
            {
                FOS_System.Heap.Free(cbw);
                if (FreeStatusBuffer)
                {
                    FOS_System.Heap.Free(statusBuffer);
                }
            }
        }

        public void SendSCSI_SyncCacheCommand(bool SyncV, bool ImmediateResponse, void* statusBuffer)
        {
            //0 block count means all blocks after specified LBA.
            //Thus, using LBA=0 & Blocks=0, syncs all blocks on the device!
            SendSCSI_SyncCacheCommand(SyncV, ImmediateResponse, 0, 0, statusBuffer);
        }
        public void SendSCSI_SyncCacheCommand(bool SyncV, bool ImmediateResponse, uint LBA, ushort Blocks, void* statusBuffer)
        {
            #if MSD_TRACE
            DBGMSG("SyncCache Command");
            DBGMSG("OUT part");
            DBGMSG(((FOS_System.String)"Toggle OUT ") + ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).toggle);
            #endif

            CommandBlockWrapper* cbw = (CommandBlockWrapper*)FOS_System.Heap.AllocZeroed((uint)sizeof(CommandBlockWrapper));
            bool FreeStatusBuffer = false;
            //try
            {
                SetupSCSICommand(0x35, cbw, LBA, Blocks);
                cbw->commandByte[1] = (byte)((SyncV ? 0x4 : 0) | (ImmediateResponse ? 0x2 : 0));

                #if MSD_TRACE
                DBGMSG("Setup transfer...");
                #endif
                USBTransfer transfer = new USBTransfer();
                DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_OUTEndpointID, 512);
                DeviceInfo.hc.OUTTransaction(transfer, false, cbw, 31);
                DeviceInfo.hc.IssueTransfer(transfer);


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
                    DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_INEndpointID, 512);
                    #if MSD_TRACE
                    DBGMSG("Done.");
                    #endif
                    DeviceInfo.hc.INTransaction(transfer, false, statusBuffer, 13);
                    #if MSD_TRACE
                    DBGMSG("Issue transfer...");
                    #endif
                    DeviceInfo.hc.IssueTransfer(transfer);
                    #if MSD_TRACE
                    DBGMSG("Done.");
                    DBGMSG("Check command...");
                    #endif

                    if (!transfer.success || CheckSCSICommand(statusBuffer, 0x35) != 0)
                    {
                        // TODO: Handle failure/timeout
                        #if MSD_TRACE
                        DBGMSG("SCSI SyncCaches (10) (In) command failed!");
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
                    DBGMSG("SCSI SyncCache (10) (Out) command failed!");
                    #endif
                }
            }
            //finally
            {
                FOS_System.Heap.Free(cbw);
                if (FreeStatusBuffer)
                {
                    FOS_System.Heap.Free(statusBuffer);
                }
            }
        }

        /// <summary>
        /// The maximum number of times to resend the "ready" query. See TestDeviceReady().
        /// </summary>
        /// <see cref="TestDeviceReady()"/>
        public static byte MaxReadyTests = 5;
        /// <summary>
        /// Tests the device is ready to send/receive data.
        /// </summary>
        /// <returns>The last status byte from the device.</returns>
        public byte TestDeviceReady()
        {
            uint timeout = MaxReadyTests;
            byte statusByte = 0;

            while (timeout != 0)
            {
                timeout--;
#if MSD_TRACE
                DBGMSG("SCSI: test unit ready");
#endif

                byte* statusBuffer = (byte*)FOS_System.Heap.AllocZeroed(13u);
                //try
                {
                    SendSCSICommand_IN(0x00, 0u, 0, null, statusBuffer); // dev, endp, cmd, LBA, transfer length

                    byte statusByteTestReady = (byte)*(((uint*)statusBuffer) + 3);

                    if (timeout >= MaxReadyTests / 2 && statusByteTestReady != 0) continue;

#if MSD_TRACE
                DBGMSG("SCSI: request sense");
#endif

                    byte* dataBuffer = (byte*)FOS_System.Heap.AllocZeroed(18u);
                    //try
                    {
                        SendSCSICommand_IN(0x03, 0, 18, dataBuffer, statusBuffer); // dev, endp, cmd, LBA, transfer length

                        statusByte = (byte)*(((uint*)statusBuffer) + 3);

                        int sense = GetRequestSense(dataBuffer);
                        if (sense == 0 || sense == 6)
                        {
                            break;
                        }
                    }
                    //finally
                    {
                        FOS_System.Heap.Free(dataBuffer);
                    }
                }
                //finally
                {
                    FOS_System.Heap.Free(statusBuffer);
                }
            }

            return statusByte;
        }
        /// <summary>
        /// Analyses the inquiry data to extract device-specific information.
        /// </summary>
        /// <param name="inquiryData">A pointer to the data to analyse.</param>
        /// <see cref="!:http://en.wikipedia.org/wiki/SCSI_Inquiry_Command"/>
        static void AnalyseInquiry(byte* inquiryData)
        {
            //TODO: Store this data in the class for later

            // cf. Jan Axelson, USB Mass Storage, page 140
            byte PeripheralDeviceType = Utils.GetField(inquiryData, 0, 0, 5); // byte, shift, len
            // uint8_t PeripheralQualifier  = getField(addr, 0, 5, 3);
            // uint8_t DeviceTypeModifier   = getField(addr, 1, 0, 7);
            byte RMB = Utils.GetField(inquiryData, 1, 7, 1);
#if MSD_TRACE
            byte ANSIapprovedVersion = Utils.GetField(inquiryData, 2, 0, 3);
#endif
            // uint8_t ECMAversion          = getField(addr, 2, 3, 3);
            // uint8_t ISOversion           = getField(addr, 2, 6, 2);
            byte ResponseDataFormat = Utils.GetField(inquiryData, 3, 0, 4);
            byte HISUP = Utils.GetField(inquiryData, 3, 4, 1);
            byte NORMACA = Utils.GetField(inquiryData, 3, 5, 1);
            // uint8_t AdditionalLength     = getField(addr, 4, 0, 8);
            byte CmdQue = Utils.GetField(inquiryData, 7, 1, 1);
            byte Linked = Utils.GetField(inquiryData, 7, 3, 1);

#if MSD_TRACE || DEVICE_INFO
            BasicConsole.WriteLine("Vendor ID  : " + FOS_System.ByteConverter.GetASCIIStringFromASCII(inquiryData, 8, 8));
            BasicConsole.WriteLine("Product ID : " + FOS_System.ByteConverter.GetASCIIStringFromASCII(inquiryData, 16, 16));

            DBGMSG("Revision   : " + FOS_System.ByteConverter.GetASCIIStringFromASCII(inquiryData, 32, 4));

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

//        public void TestMSD()
//        {
//            // maxLUN (0 for USB-sticks)
//            DeviceInfo.maxLUN = 0;

//            // start with correct endpoint toggles and reset interface
//            ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).toggle = false;
//            ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_INEndpointID]).toggle = false;

//            BulkReset(DeviceInfo.MSD_InterfaceNum); // Reset Interface

//            ///////// send SCSI command "inquiry (opcode: 0x12)"
//#if MSD_TRACE
//            DBGMSG("SCSI: inquiry");
//#endif
              
//#if DEVICE_INFO || MSD_TRACE
//            BasicConsole.DelayOutput(10);
//#endif

//            ///////// send SCSI command "test unit ready(6)"
//            TestDeviceReady();

//            ///////// send SCSI command "read capacity(10)"
//#if MSD_TRACE
//            DBGMSG("SCSI: read capacity");
//#endif

//            uint* capacityBuffer = (uint*)FOS_System.Heap.AllocZeroed(8);
//            SendSCSICommand_IN(0x25 /*SCSI opcode*/, 0 /*LBA*/, 8 /*Bytes In*/, capacityBuffer, null);

//            // MSB ... LSB
//            capacityBuffer[0] = Utils.htonl(capacityBuffer[0]);
//            capacityBuffer[1] = Utils.htonl(capacityBuffer[1]);

//            //diskDevice.SetBlockCount(((ulong)capacityBuffer[0]) + 1);
//            //diskDevice.SetBlockSize((ulong)capacityBuffer[1]);
            
//#if MSD_TRACE
//            DBGMSG(((FOS_System.String)"Capacity: ") + (diskDevice.BlockCount * diskDevice.BlockSize) + ", Last LBA: " + capacityBuffer[0] + ", block size: " + capacityBuffer[1]);
//#endif
//        }

        /// <summary>
        /// Reads the specified block of the device.
        /// </summary>
        /// <param name="sector">The sector index to read.</param>
        /// <param name="buffer">The buffer to store the sector data in. Must be at least as big as the sector size.</param>
        /// <returns>True if the read was successful. Otherwise false.</returns>
        public bool Read(uint sector, void* buffer)
        {
#if MSD_TRACE
            DBGMSG(((FOS_System.String)">SCSI: read sector: ") + sector);
#endif

            SendSCSICommand_IN(0x28 /*SCSI opcode*/, sector /*LBA*/, (ushort)diskDevice.BlockSize /*Bytes In*/, buffer, null);

            return true;
        }
        /// <summary>
        /// Writes the specified block of the device.
        /// </summary>
        /// <param name="sector">The sector index to write.</param>
        /// <param name="buffer">The buffer storing the data to write. Must be at least as big as the sector size.</param>
        /// <returns>True if the write was successful. Otherwise false.</returns>
        public bool Write(uint sector, void* buffer)
        {
#if MSD_TRACE
            DBGMSG(((FOS_System.String)">SCSI: write sector: ") + sector);
#endif

            SendSCSICommand_OUT(0x2A /*SCSI opcode*/, sector /*LBA*/, (ushort)diskDevice.BlockSize /*Bytes Out*/, buffer, null);

            return true;
        }

        /// <summary>
        /// Performs the reset-recovery operation on the specified interface.
        /// </summary>
        /// <param name="Interface">The interface to reset.</param>
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
            ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).toggle = false;
            ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_INEndpointID]).toggle = false;
            BulkReset(DeviceInfo.MSD_InterfaceNum); // Reset Interface
        }
        
        /// <summary>
        /// Human readable descriptions of the SCSI command sense keys.
        /// </summary>
        public static FOS_System.String[] SenseDescriptions =
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
        /// <summary>
        /// Gets the request's result's sense. 
        /// </summary>
        /// <param name="RequestResultData">The result data to get the sense from.</param>
        /// <returns>The sense key.</returns>
        public static int GetRequestSense(byte* RequestResultData)
        {
            uint Valid = Utils.GetField(RequestResultData, 0, 7, 1); // byte 0, bit 7
            uint ResponseCode = Utils.GetField(RequestResultData, 0, 0, 7); // byte 0, bit 6:0
            uint SenseKey = Utils.GetField(RequestResultData, 2, 0, 4); // byte 2, bit 3:0
            
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
                if (SenseKey < SenseDescriptions.Length)
                    DBGMSG(SenseDescriptions[SenseKey]);
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
    /// <summary>
    /// Represents a mass storage device as a disk device. Allows an MSD to be accessed
    /// like a normal disk device.
    /// </summary>
    public unsafe class MassStorageDevice_DiskDevice : DiskDevice
    {
        /// <summary>
        /// The MSD that the disk device is an interface for.
        /// </summary>
        protected MassStorageDevice msd;

        /// <summary>
        /// Initialises a new disk device interface for the specified MSD. 
        /// Also determines the device's maximum size.
        /// </summary>
        /// <param name="anMSD">The MSD to create an interface for.</param>
        public MassStorageDevice_DiskDevice(MassStorageDevice anMSD)
        {
            msd = anMSD;
            
            uint* capacityBuffer = (uint*)FOS_System.Heap.AllocZeroed(8);
            //try
            {
                anMSD.SendSCSICommand_IN(0x25 /*SCSI opcode*/, 0 /*LBA*/, 8 /*Bytes In*/, capacityBuffer, null);

                // MSB ... LSB
                capacityBuffer[0] = Utils.htonl(capacityBuffer[0]);
                capacityBuffer[1] = Utils.htonl(capacityBuffer[1]);

                blockCount = ((ulong)capacityBuffer[0]) + 1;
                blockSize = (ulong)capacityBuffer[1];
            }
            //finally
            {
                FOS_System.Heap.Free(capacityBuffer);
            }

            DeviceManager.Devices.Add(this);
        }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="aBlockNo">See base class.</param>
        /// <param name="aBlockCount">See base class.</param>
        /// <param name="aData">See base class.</param>
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
        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="aBlockNo">See base class.</param>
        /// <param name="aBlockCount">See base class.</param>
        /// <param name="aData">See base class.</param>
        public override void WriteBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            if (aData == null)
            {
                byte* dataPtr = ((byte*)Utilities.ObjectUtilities.GetHandle(NewBlockArray(1))) + FOS_System.Array.FieldsBytesSize;
                for (uint i = 0; i < aBlockCount; i++)
                {
                    msd.Write((uint)(aBlockNo + i), dataPtr);

                    FOS_System.GC.Cleanup();
                }
            }
            else
            {
                byte* dataPtr = ((byte*)Utilities.ObjectUtilities.GetHandle(aData)) + FOS_System.Array.FieldsBytesSize;
                for (uint i = 0; i < aBlockCount; i++)
                {
                    msd.Write((uint)(aBlockNo + i), dataPtr);
                    dataPtr += blockSize;

                    FOS_System.GC.Cleanup();
                }
            }
        }

        public void Test()
        {
            try
            {
                int dataSize = 4096;
                BasicConsole.WriteLine("Testing MSD...");

                BasicConsole.WriteLine("Setting up write data...");
                byte[] writeData = new byte[512];
                for (int i = 0; i < writeData.Length; i++)
                {
                    writeData[i] = (byte)(i % 255);
                }
                for (int i = 0; i < (dataSize / 512); i++)
                {
                    BasicConsole.WriteLine("Writing data...");
                    WriteBlock((uint)i, 1, null);
                }

                BasicConsole.WriteLine("Syncing caches...");
                msd.SendSCSI_SyncCacheCommand(false, false, null);

                byte[] readData = new byte[512];
                bool DataOK = true;
                for (int i = 0; i < (dataSize / 512); i++)
                {
                    BasicConsole.WriteLine("Reading data...");
                    ReadBlock((uint)i, 1, readData);
                    BasicConsole.WriteLine("Checking data...");

                    for (int j = 0; j < 512; j++)
                    {
                        DataOK = (readData[j] == 0);//(j % 255));
                        readData[j] = 0;
                        if (!DataOK)
                        {
                            break;
                        }
                    }

                    if (DataOK)
                    {
                        BasicConsole.WriteLine("Data OK.");
                    }
                    else
                    {
                        BasicConsole.WriteLine("Data not OK.");
                    }

                    if(!DataOK)
                    {
                        break;
                    }
                }
            }
            catch
            {
                BasicConsole.WriteLine("Exception. Test failed.");
            }
            BasicConsole.DelayOutput(5);
        }

        /// <summary>
        /// Destroys the MSD disk device.
        /// </summary>
        public void Destroy()
        {
            DeviceManager.Devices.Remove(this);
            msd = null;
        }
    }

    /// <summary>
    /// The command block wrapper structure which makes up a SCSI command.
    /// </summary>
    public unsafe struct CommandBlockWrapper
    {
        /// <summary>
        /// Read the spec.
        /// </summary>
        public uint CBWSignature;
        /// <summary>
        /// Read the spec.
        /// </summary>
        public uint CBWTag;
        /// <summary>
        /// Read the spec.
        /// </summary>
        public uint CBWDataTransferLength;
        /// <summary>
        /// Read the spec.
        /// </summary>
        public byte CBWFlags;
        /// <summary>
        /// Read the spec.
        /// </summary>
        public byte CBWLUN;           // only bits 3:0
        /// <summary>
        /// Read the spec.
        /// </summary>
        public byte CBWCBLength;      // only bits 4:0
        /// <summary>
        /// Read the spec.
        /// </summary>
        public fixed byte commandByte[16];
    }
}
