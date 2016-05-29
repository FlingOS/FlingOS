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

//#define MSD_TRACE
//#define DEVICE_INFO

using Kernel.Devices;
using Kernel.Devices.Controllers;
using Kernel.Framework;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Devices;
using Kernel.Utilities;
using Utils = Kernel.Utilities.MemoryUtils;

namespace Kernel.USB.Devices
{
    /// <summary>
    ///     Constant values used by the Mass Storage Device driver.
    /// </summary>
    public static class MassStorageDevice_Consts
    {
        /// <summary>
        ///     Indicates the SCSI command is not OK.
        /// </summary>
        public const uint CSWMagicNotOK = 0x01010101;

        /// <summary>
        ///     Indiciates the SCSI command is OK.
        /// </summary>
        public const uint CSWMagicOK = 0x53425355; // USBS

        /// <summary>
        ///     SCSI command packet siganture.
        /// </summary>
        public const uint CBWMagic = 0x43425355; // USBC
    }

    /// <summary>
    ///     All the possible Mass Storage Device power states.
    /// </summary>
    public enum MSD_PowerStates : byte
    {
        /// <summary>
        ///     Default state used with devices which do not support the other states.
        ///     Tells the device to process the Start and LOEJ bits.
        /// </summary>
        /// <remarks>
        ///     If you set the power state to something other than START_VALID, the Start
        ///     and LOEJ bits are ignored.
        /// </remarks>
        START_VALID = 0x0,

        /// <summary>
        ///     Tells the device to transition into the Active power state.
        /// </summary>
        ACTIVE = 0x1,

        /// <summary>
        ///     Tells the device to transition into the Idle power state.
        /// </summary>
        IDLE = 0x2,

        /// <summary>
        ///     Tells the device to transition into the Standby power state.
        /// </summary>
        STANDBY = 0x3,

        /// <summary>
        ///     Tells the device to transfer power control to the logical unit.
        /// </summary>
        /// <remarks>
        ///     Enables the Idle timer and disables the Standby timer.
        /// </remarks>
        LU_CONTROL = 0x7,

        /// <summary>
        ///     Forces the Idle timer to 0 then transitions the device power state to
        ///     Idle then hands power control back to the device server.
        /// </summary>
        FORCE_IDLE_0 = 0xA,

        /// <summary>
        ///     Forces the Standby timer to 0 then transitions the device power state to
        ///     Standby then hands power control back to the device server.
        /// </summary>
        FORCE_STANDBY_0 = 0xB
    }

    /// <summary>
    ///     Represents a USB mass storage device.
    /// </summary>
    /// <remarks>
    ///     The SCSI MSD commands specification is available from
    ///     <a href="http://www.seagate.com/staticfiles/support/disc/manuals/scsi/100293068a.pdf">Seagate</a>.
    /// </remarks>
    public unsafe class MassStorageDevice : USBDevice
    {
        //TODO: Test for ejected, don't send commands if ejected!

        /// <summary>
        ///     The underlying disk device that allows the mass storage device to be
        ///     accessed in the same way as a disk.
        /// </summary>
        public MassStorageDevice_DiskDevice diskDevice { get; protected set; }

        /// <summary>
        ///     Whether the device has been ejected or not.
        /// </summary>
        public bool Ejected { get; protected set; }

        /// <summary>
        ///     Whether the device is in the Active power state or not.
        /// </summary>
        public bool Active { get; protected set; }

        /// <summary>
        ///     Initialises a new mass storage device with the specified USB device information.
        /// </summary>
        /// <param name="aDeviceInfo">
        ///     The USB device information that specifies the physical mass storage device.
        /// </param>
        public MassStorageDevice(USBDeviceInfo aDeviceInfo)
            : base(aDeviceInfo, DeviceGroup.Storage, DeviceClass.Storage, DeviceSubClass.USB, "USB Mass Storage", true)
        {
#if MSD_TRACE
            DBGMSG("------------------------------ Mass Storage Device -----------------------------");
            DBGMSG(((Framework.String)"MSD Interface num: ") + DeviceInfo.MSD_InterfaceNum);
            BasicConsole.DelayOutput(1);
#endif
            //Immediately set up the MSD.
            Setup();
        }

        /// <summary>
        ///     Sets up the mass storage device for bulk transfer. Also creates the mass storage disk device.
        /// </summary>
        protected void Setup()
        {
            // Start with correct endpoint toggles
            ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).Toggle = false;
            ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_INEndpointID]).Toggle = false;

            // Reset the main MSD interface
            BulkReset(DeviceInfo.MSD_InterfaceNum);

            //Check the device is responding correctly. The inquiry will also return
            //  you more information like Id string indices etc. but we just check 
            //  then discard them for now.
            byte* inquiryBuffer = (byte*)Heap.AllocZeroed(26u, "MassStorageDevice : Setup");
            try
            {
                SendSCSICommand_IN(0x12 /*SCSI opcode*/, 0 /*LBA*/, 36 /*Bytes In*/, inquiryBuffer, null);

                AnalyseInquiry(inquiryBuffer);
            }
            finally
            {
                Heap.Free(inquiryBuffer);
            }

            // Send SCSI command "test unit ready(6)"
            //  And check the response is OK
            if (TestDeviceReady() != 0)
            {
                ExceptionMethods.Throw(new Exception("Mass Storage Device not ready!"));
            }

            // Create the paired disk device for this MSD.
            diskDevice = new MassStorageDevice_DiskDevice(this);
            DeviceManager.RegisterDevice(diskDevice);

            // Tell the MSD device to "load" the medium
            Load();
            // Then force the code to put the MSD into the Idle power state
            Idle(true);

            // Initialise a thread to control the interface to the disk
            StorageController.Init();
            StorageController.AddDisk(diskDevice);
        }

        /// <summary>
        ///     Destroys the USB device.
        /// </summary>
        public override void Destroy()
        {
            // Safely eject the device before destroying / allowing user
            //  to unplug.
            Eject();

            // Destroy the paired disk device.
            diskDevice.Destroy();
            diskDevice = null;

            // Destroy this
            base.Destroy();
        }

        /// <summary>
        ///     Transitions the device to the Active power state.
        /// </summary>
        public void Activate()
        {
            if (!Active)
            {
                Active = true;

                //As per spec, send Start-Stop Unit command:
                //  - No immediate response - wait for command to complete before returning
                //  - Set to Active power state
                //  - If ACTIVE is ignored, makes sure we set Start and LoadEject options
                //    to have the device start and load the medium
                SendSCSI_StartStopUnitCommand(false, MSD_PowerStates.ACTIVE, true, true, null);
            }
        }

        /// <summary>
        ///     Transitions the device to the Idle power state.
        /// </summary>
        /// <param name="overrideCondition">
        ///     Whether to ignore the internal Active state or not.
        /// </param>
        public void Idle(bool overrideCondition)
        {
            //We may want to force our code to put the device into Idle
            //  power state. It is not against the SCSI Spec to tell a device
            //  to transition to its current power state.
            if (Active || overrideCondition)
            {
                Active = false;

                //As per spec, send Start-Stop Unit command:
                //  - No immediate response - wait for command to complete before returning
                //  - Set to Idle power state
                //  - If IDLE is ignored, makes sure we clear Start and LoadEject options
                //    to have the device stop but not eject the medium.
                SendSCSI_StartStopUnitCommand(false, MSD_PowerStates.IDLE, false, false, null);
            }
        }

        /// <summary>
        ///     Transitions the device to the Standby power state.
        /// </summary>
        public void Standby()
        {
            Active = false;

            //As per spec, send Start-Stop Unit command:
            //  - No immediate response - wait for command to complete before returning
            //  - Set to Standby power state
            //  - If STANDBY is ignored, makes sure we clear Start and LoadEject options
            //    to have the device stop but not eject the medium.
            SendSCSI_StartStopUnitCommand(false, MSD_PowerStates.STANDBY, false, false, null);
        }

        /// <summary>
        ///     Tells the device to laod the medium.
        /// </summary>
        public void Load()
        {
            if (Ejected)
            {
                Ejected = false;

                //As per spec, send Start-Stop Unit command:
                //  - No immediate response - wait for command to complete before returning
                //  - Tell the device to use the START and LOEJ options
                //  - Set Start and LoadEject options to have the device start and load the medium.
                SendSCSI_StartStopUnitCommand(false, MSD_PowerStates.START_VALID, true, true, null);
            }
        }

        /// <summary>
        ///     Safely ejects the device.
        /// </summary>
        public void Eject()
        {
            if (!Ejected)
            {
                Ejected = true;

                //Note: The eject command also makes the device perform (internally)
                //      a sync of non-volatile and volatile caches so it is not
                //      necessary to send the SyncCaches command before sending the 
                //      eject command.

                //As per spec, send Start-Stop Unit command:
                //  - No immediate response - wait for command to complete before returning
                //  - Tell the device to use the START and LOEJ options
                //  - Clear Start and LoadEject options to have the device stop but not eject the medium.
                SendSCSI_StartStopUnitCommand(false, MSD_PowerStates.START_VALID, false, true, null);
            }
        }

        /// <summary>
        ///     Sends the synchronise cache command to tell the device
        ///     to immediately write any cached data to the medium.
        /// </summary>
        public void CleanCaches()
        {
            if (!Ejected)
            {
                //Not sure if this is strictly necessary but it would make
                //  sense that the device must be active and have the medium
                //  loaded for it to be able to sync caches (i.e. write) to 
                //  the medium.
                bool goIdle = false;
                if (!Active)
                {
                    Activate();
                    //Store the fact that this method activated the device.
                    //  It is possible the device is already active and we are
                    //  simply performing a sync cache command during some 
                    //  complex operation. In which case we wouldn't want to 
                    //  idle the device and interrupt said complex operation.
                    goIdle = true;
                }

                // As per spec, send the CleanCaches command:
                //  - No immediate response - wait for the command to complete before returning
                //  - Clear SyncNV : Will sync both non-volate and volatile caches to disk.
                SendSCSI_SyncCacheCommand(false, false, null);

                // Only idle the device if this method was the one which activated it.
                //  See above to reasoning.
                if (goIdle)
                {
                    Idle(false);
                }
            }
        }

        /// <summary>
        ///     Resets the bulk tarnsfer interface.
        /// </summary>
        public void BulkReset()
        {
            BulkReset(DeviceInfo.MSD_InterfaceNum);
        }

        /// <summary>
        ///     Resets the bulk tarnsfer interface.
        /// </summary>
        /// <param name="numInterface">The bulk transfer interface number to reset.</param>
        public void BulkReset(byte numInterface)
        {
#if MSD_TRACE
            DBGMSG(((Framework.String)"USB MSD: TransferBulkOnly - MassStorageReset, interface: ") + numInterface);
#endif

            USBTransfer transfer = new USBTransfer();
            DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Control, 0, 64);

            // RequestType   Request   Value  Index     Length   Data
            // 00100001b     11111111b 0000h  Interface 0000h    None
            DeviceInfo.hc.SETUPTransaction(transfer, 8, 0x21, 0xFF, 0, 0, numInterface, 0);
            DeviceInfo.hc.INTransaction(transfer, true, null, 0); // Handshake
            DeviceInfo.hc.IssueTransfer(transfer);
        }

        /// <summary>
        ///     Sets up a SCSI command.
        /// </summary>
        /// <param name="SCSICommand">The command byte.</param>
        /// <param name="cbw">A pointer to an existing command block wrapper.</param>
        /// <param name="LBA">The LBA to access.</param>
        /// <param name="TransferLength">The length of the data in bytes.</param>
        public void SetupSCSICommand(byte SCSICommand, CommandBlockWrapper* cbw, uint LBA, ushort TransferLength)
        {
            // Magic (signature) bytes
            cbw->CBWSignature = MassStorageDevice_Consts.CBWMagic;
            // Device echoes this field in the CSWTag field of the associated CSW
            cbw->CBWTag = 0x42424200u | SCSICommand;
            // Transfer data length in bytes (only data!)
            cbw->CBWDataTransferLength = TransferLength;
            // Operation code
            cbw->commandByte[0] = SCSICommand;
            switch (SCSICommand)
            {
                // Test Unit Ready (6)
                case 0x00:
                    // Out: 0x00  In: 0x80
                    cbw->CBWFlags = 0x00;
                    cbw->CBWCBLength = 6;
                    break;
                // Request Sense (6)
                case 0x03:
                    // Out: 0x00  In: 0x80
                    cbw->CBWFlags = 0x80;
                    cbw->CBWCBLength = 6;
                    // Allocation length (max. bytes)
                    cbw->commandByte[4] = 18;
                    break;
                // Inquiry (6)
                case 0x12:
                    // Out: 0x00  In: 0x80
                    cbw->CBWFlags = 0x80;
                    cbw->CBWCBLength = 6;
                    // Allocation length (max. bytes)
                    cbw->commandByte[4] = 36;
                    break;
                // Read Capacity (10)
                case 0x25:
                    // Out: 0x00  In: 0x80
                    cbw->CBWFlags = 0x80;
                    cbw->CBWCBLength = 10;
                    cbw->commandByte[2] = (byte)(LBA >> 24); // LBA MSB
                    cbw->commandByte[3] = (byte)(LBA >> 16); //   ...
                    cbw->commandByte[4] = (byte)(LBA >> 8); //   ...
                    cbw->commandByte[5] = (byte)LBA; // LBA LSB
                    break;
                // Read (10)
                case 0x28:
                    // Out: 0x00  In: 0x80
                    cbw->CBWFlags = 0x80;
                    cbw->CBWCBLength = 10;
                    cbw->commandByte[2] = (byte)(LBA >> 24); // LBA MSB
                    cbw->commandByte[3] = (byte)(LBA >> 16); //   ...
                    cbw->commandByte[4] = (byte)(LBA >> 8); //   ...
                    cbw->commandByte[5] = (byte)LBA; // LBA LSB
                    cbw->commandByte[7] = (byte)((TransferLength/(uint)diskDevice.BlockSize) >> 8);
                    // MSB <--- blocks not byte!
                    cbw->commandByte[8] = (byte)(TransferLength/(uint)diskDevice.BlockSize); // LSB
                    break;
                // Write (10)
                case 0x2A:
                    // Out: 0x00  In: 0x80
                    cbw->CBWFlags = 0x00;
                    cbw->CBWCBLength = 10;
                    cbw->commandByte[2] = (byte)(LBA >> 24); // LBA MSB
                    cbw->commandByte[3] = (byte)(LBA >> 16); //   ...
                    cbw->commandByte[4] = (byte)(LBA >> 8); //   ...
                    cbw->commandByte[5] = (byte)LBA; // LBA LSB
                    cbw->commandByte[7] = (byte)((TransferLength/(uint)diskDevice.BlockSize) >> 8);
                    // MSB <--- blocks not byte!
                    cbw->commandByte[8] = (byte)(TransferLength/(uint)diskDevice.BlockSize); // LSB
                    break;
                // Synchronise Cache (10)
                case 0x35:
                    // Out: 0x00  In: 0x80
                    cbw->CBWFlags = 0x00;
                    cbw->CBWCBLength = 10;
                    cbw->CBWDataTransferLength = 0;
                    cbw->commandByte[2] = (byte)(LBA >> 24); // LBA MSB
                    cbw->commandByte[3] = (byte)(LBA >> 16); //   ...
                    cbw->commandByte[4] = (byte)(LBA >> 8); //   ...
                    cbw->commandByte[5] = (byte)LBA; // LBA LSB
                    cbw->commandByte[7] = (byte)((TransferLength/(uint)diskDevice.BlockSize) >> 8);
                    // MSB <--- blocks not byte!
                    cbw->commandByte[8] = (byte)(TransferLength/(uint)diskDevice.BlockSize); // LSB
                    break;
                // Start Stop Unit (6)
                case 0x1B:
                    // Out: 0x00  In: 0x80
                    cbw->CBWFlags = 0x00;
                    cbw->CBWCBLength = 6;
                    cbw->CBWDataTransferLength = 0;
                    break;
            }
        }

        /// <summary>
        ///     Checks a received SCSI command is valid and OK.
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
            uint CSWtag = *((uint*)MSDStatus + 1); // DWORD 1 (byte 4:7)

            if (((byte)CSWtag == SCSIOpcode) && ((byte)(CSWtag >> 8) == 0x42) && ((byte)(CSWtag >> 16) == 0x42) &&
                ((byte)(CSWtag >> 24) == 0x42))
            {
#if MSD_TRACE
                DBGMSG(((Framework.String)"CSW tag ") + (byte)(CSWtag) + " OK");
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
            uint CSWDataResidue = *((uint*)MSDStatus + 2); // DWORD 2 (byte 8:11)
            if (CSWDataResidue == 0)
            {
#if MSD_TRACE
                DBGMSG("CSW data residue OK");
#endif
            }
            else
            {
#if MSD_TRACE
                DBGMSG(((Framework.String)"CSW data residue: ") + CSWDataResidue);
#endif
            }

            // check status byte // DWORD 3 (byte 12)
            byte CSWstatusByte = *((byte*)MSDStatus + 12); // byte 12 (last byte of 13 bytes)

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
                    ResetRecoveryMSD();
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
        ///     Sends a SCSI command that receives data.
        /// </summary>
        /// <param name="SCSIcommand">The SCSI command to send.</param>
        /// <param name="LBA">The LBA to access.</param>
        /// <param name="TransferLength">The length of the data to receive.</param>
        /// <param name="dataBuffer">The data buffer - must be at least as big as the transfer length.</param>
        /// <param name="statusBuffer">The buffer to store the command status result in. Must be at least 13 bytes long.</param>
        /// <see cref="!:http://www.beyondlogic.org/usbnutshell/usb4.htm#Bulk" />
        public bool SendSCSICommand_IN(byte SCSIcommand, uint LBA, ushort TransferLength, void* dataBuffer,
            void* statusBuffer, bool resetOnFail = true)
        {
#if MSD_TRACE
            DBGMSG("OUT part");
            DBGMSG(((Framework.String)"Toggle OUT ") + ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).toggle);
#endif
            bool OK = true;

            //Allocate memory for the command block
            //  This is passed to the out transaction as the SCSI command data.
#if MSD_TRACE
            BasicConsole.Write("CBW Size: ");
            BasicConsole.Write(sizeof(CommandBlockWrapper));
            if(sizeof(CommandBlockWrapper) != 31)
            {
                BasicConsole.WriteLine(" - INCORRECT! FAILED!");
            }
            else
            {
                BasicConsole.WriteLine(" - correct.");
            }
#endif
            CommandBlockWrapper* cbw =
                (CommandBlockWrapper*)
                    Heap.AllocZeroed((uint)sizeof(CommandBlockWrapper), "MassStorageDevice : SendSCSICommand_IN (1)");
            bool FreeStatusBuffer = false;
            try
            {
                //Initialise the command data
                SetupSCSICommand(SCSIcommand, cbw, LBA, TransferLength);
#if MSD_TRACE
                BasicConsole.WriteLine("cbw: ");
                BasicConsole.DumpMemory(cbw, sizeof(CommandBlockWrapper));
                DBGMSG("Setup command. Transferring data...");
#endif
                // Create a new USB transfer 
                //  This transfer is re-used for all transfers throughout this method
                USBTransfer transfer = new USBTransfer();

                // Initialises the transfer
                //  Sets the transfer packet size, points it to the MSD OUT endpoint and sets it as a bulk transfer
                DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_OUTEndpointID,
                    512);
                // Adds an OUT transaction to the transfer.
                //  This OUT transaction ouptuts the SCSI command to the MSD
                DeviceInfo.hc.OUTTransaction(transfer, false, cbw, 31);
                // Issues the complete transfer to the device.
                DeviceInfo.hc.IssueTransfer(transfer);

                // If the transfer completed successfully.
                if (transfer.success)
                {
#if MSD_TRACE
                    DBGMSG("IN part");
#endif

                    // If the caller didn't provide a pre-allocated status buffer, we
                    //  must allocate one.
                    if (statusBuffer == null)
                    {
#if MSD_TRACE
                        DBGMSG("Alloc 13 bytes of mem...");
#endif
                        // And we must remember to only free it later if we created it.
                        FreeStatusBuffer = true;
                        // Create the pre-allocated buffer. Size 13 is the size of the response.
                        statusBuffer = Heap.AllocZeroed(13u, "MassStorageDevice : SendSCSICommand_IN (2)");
                    }

#if MSD_TRACE
                    DBGMSG("Setup transfer...");
#endif
                    // Setup a new transfer to receive the response from the device
                    DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_INEndpointID,
                        512);
#if MSD_TRACE
                    DBGMSG("Done.");
#endif
                    // If the amount of data to receive is greater than 0 bytes in length:
                    if (TransferLength > 0)
                    {
#if MSD_TRACE
                        DBGMSG("Setup IN transactions...");
#endif
                        // We must do an IN transaction to receive the data from the device
                        DeviceInfo.hc.INTransaction(transfer, false, dataBuffer, TransferLength);
                        // And then do the normal IN transaction to receive the status response.
                        DeviceInfo.hc.INTransaction(transfer, false, statusBuffer, 13);
#if MSD_TRACE
                        DBGMSG("Done.");
#endif
                    }
                    else
                    {
                        // No data to receive so just do the IN transaction to receive the status 
                        //  response.
                        DeviceInfo.hc.INTransaction(transfer, false, statusBuffer, 13);
                    }
#if MSD_TRACE
                    DBGMSG("Issue transfer...");
#endif
                    // Issue the transfer of IN data.
                    DeviceInfo.hc.IssueTransfer(transfer);
#if MSD_TRACE
                    DBGMSG("Done.");
                    DBGMSG("Check command...");
#endif
                    // If the transfer failed or the status response indicates failure of some form:
                    if (!transfer.success || CheckSCSICommand(statusBuffer, SCSIcommand) != 0)
                    {
                        // TODO: Handle failure/timeout
#if MSD_TRACE
                        DBGMSG("SCSI IN command failed!");
#endif
                        OK = false;

                        if (resetOnFail)
                        {
                            ResetRecoveryMSD();
                        }
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
                    OK = false;
#if MSD_TRACE
                    DBGMSG("SCSI OUT command failed!");
#endif
                }
            }
            finally
            {
                Heap.Free(cbw);
                // Only free the status buffer if we allocated it
                if (FreeStatusBuffer)
                {
                    Heap.Free(statusBuffer);
                }
            }

            return OK;
        }

        /// <summary>
        ///     Sends a SCSI command that sends data.
        /// </summary>
        /// <param name="SCSIcommand">The SCSI command to send.</param>
        /// <param name="LBA">The LBA to access.</param>
        /// <param name="TransferLength">The length of the data to send.</param>
        /// <param name="dataBuffer">The data buffer - must be at least as long as the transfer length.</param>
        /// <param name="statusBuffer">The data buffer to store the command status result in. Must be at least 13 bytes long.</param>
        public void SendSCSICommand_OUT(byte SCSIcommand, uint LBA, ushort TransferLength, void* dataBuffer,
            void* statusBuffer)
        {
#if MSD_TRACE
            DBGMSG("OUT part");
            DBGMSG(((Framework.String)"Toggle OUT ") + ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).toggle);
#endif

            //This method work pretty much the same as the SendSCSICommand_IN method so see there for docs.

            CommandBlockWrapper* cbw =
                (CommandBlockWrapper*)
                    Heap.AllocZeroed((uint)sizeof(CommandBlockWrapper), "MassStorageDevice : SendSCSICommand_OUT (1)");
            bool FreeStatusBuffer = false;
            try
            {
                SetupSCSICommand(SCSIcommand, cbw, LBA, TransferLength);

#if MSD_TRACE
                DBGMSG("Setup transfer...");
#endif
                //Issue the command transfer
                USBTransfer transfer = new USBTransfer();
                DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_OUTEndpointID,
                    512);
                DeviceInfo.hc.OUTTransaction(transfer, false, cbw, 31);
                DeviceInfo.hc.IssueTransfer(transfer);

                // If the command transfer completed successfully:
                if (transfer.success)
                {
                    //Issue the output data transfer
                    //  Through much testing / debugging I discovered that you cannot do the data OUT
                    //  transaction in the same transfer as the command OUT transaction. This seems odd
                    //  to me and I have not found any documentation or specification to back up this
                    //  behaviour. There are two possibilities here:
                    //      1) This is a genuine part of some spec / requirement, in which case we have
                    //          no issue.
                    //      2) Or my implementation for issusing multiple OUT transactions in a row in 
                    //          one is broken. In this case, I have failed to find out what causes ths issue 
                    //          / why it occurs.
                    DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_OUTEndpointID,
                        512);
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
                // If the command and data transfers completed successfully:
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
                        statusBuffer = Heap.AllocZeroed(13u, "MassStorageDevice : SendSCSICommand_OUT (2)");
                    }

#if MSD_TRACE
                    DBGMSG("Setup transfer...");
#endif
                    DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_INEndpointID,
                        512);
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
            }
            finally
            {
                Heap.Free(cbw);
                if (FreeStatusBuffer)
                {
                    Heap.Free(statusBuffer);
                }
            }
        }

        /// <summary>
        ///     Sends a SCSI Sync Cache command.
        /// </summary>
        /// <param name="SyncNV">
        ///     Whether the device is required to sync volatile and non-volatile caches or not. Specify
        ///     "false" to force full cache sync including non-volatile caches. Specify "true" to force
        ///     sync volatile caches and optionally sync non-volatile caches (up to the specific device
        ///     what it does).
        /// </param>
        /// <param name="ImmediateResponse">
        ///     Whether the device should send the status response as soon as the command is recognised
        ///     or the device should only send the response once the sync is completed. Specify "false"
        ///     for the latter option.
        /// </param>
        /// <param name="statusBuffer">
        ///     Buffer for the status response. Specify null if you don't want to keep the response data.
        /// </param>
        public void SendSCSI_SyncCacheCommand(bool SyncNV, bool ImmediateResponse, void* statusBuffer)
        {
            //0 block count means all blocks after specified LBA.
            //Thus, using LBA=0 & Blocks=0, syncs all blocks on the device!
            SendSCSI_SyncCacheCommand(SyncNV, ImmediateResponse, 0, 0, statusBuffer);
        }

        /// <summary>
        ///     Sends a SCSI Sync Cache command.
        /// </summary>
        /// <param name="SyncNV">
        ///     Whether the device is required to sync volatile and non-volatile caches or not. Specify
        ///     "false" to force full cache sync including non-volatile caches. Specify "true" to force
        ///     sync volatile caches and optionally sync non-volatile caches (up to the specific device
        ///     what it does).
        /// </param>
        /// <param name="ImmediateResponse">
        ///     Whether the device should send the status response as soon as the command is recognised
        ///     or the device should only send the response once the sync is completed. Specify "false"
        ///     for the latter option.
        /// </param>
        /// <param name="LBA">The LBA of the first block to sync.</param>
        /// <param name="Blocks">
        ///     The number of blocks to sync. Specify 0 to sync all the blocks from the start LBA through
        ///     to the end of the device.
        /// </param>
        /// <param name="statusBuffer">
        ///     Buffer for the status response. Specify null if you don't want to keep the response data.
        /// </param>
        public void SendSCSI_SyncCacheCommand(bool SyncNV, bool ImmediateResponse, uint LBA, ushort Blocks,
            void* statusBuffer)
        {
#if MSD_TRACE
            DBGMSG("SyncCache Command");
            DBGMSG("OUT part");
            DBGMSG(((Framework.String)"Toggle OUT ") + ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).toggle);
            #endif

            CommandBlockWrapper* cbw =
                (CommandBlockWrapper*)
                    Heap.AllocZeroed((uint)sizeof(CommandBlockWrapper),
                        "MassStorageDevice : SendSCSI_SyncCacheCommand (1)");
            bool FreeStatusBuffer = false;
            try
            {
                SetupSCSICommand(0x35, cbw, LBA, Blocks);
                cbw->commandByte[1] = (byte)((SyncNV ? 0x4 : 0) | (ImmediateResponse ? 0x2 : 0));

#if MSD_TRACE
                DBGMSG("Setup transfer...");
                #endif
                USBTransfer transfer = new USBTransfer();
                DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_OUTEndpointID,
                    512);
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
                        statusBuffer = Heap.AllocZeroed(13u, "MassStorageDevice : SendSCSI_SyncCacheCommand (2)");
                    }

#if MSD_TRACE
                    DBGMSG("Setup transfer...");
                    #endif
                    DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_INEndpointID,
                        512);
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
            }
            finally
            {
                Heap.Free(cbw);
                if (FreeStatusBuffer)
                {
                    Heap.Free(statusBuffer);
                }
            }
        }

        /// <summary>
        ///     Sends a SCSI START_STOP Unit command.
        /// </summary>
        /// <param name="ImmediateResponse">
        ///     Whether the device should send the status response as soon as the command is recognised
        ///     or the device should only send the response once the command processing is completed.
        ///     Specify "false" for the latter option.
        /// </param>
        /// <param name="PowerCondition">
        ///     The power condition to transition to. It is not invalid to tell the device to transition
        ///     to the same as its current power state.
        /// </param>
        /// <param name="Start">
        ///     If PowerCondition is START_VALID, tells the device to transition to the Active power state
        ///     or the Stopped power state (true for the former, false for the latter).
        /// </param>
        /// <param name="LoadEject">
        ///     False to take no load/eject action. If true then: If Start is true, the device shall load
        ///     the medium. If Start is false, the device shall eject the medium.
        /// </param>
        /// <param name="statusBuffer">
        ///     Buffer for the status response. Specify null if you don't want to keep the response data.
        /// </param>
        public void SendSCSI_StartStopUnitCommand(bool ImmediateResponse, MSD_PowerStates PowerCondition,
            bool Start, bool LoadEject, void* statusBuffer)
        {
#if MSD_TRACE
            DBGMSG("StartStop Unit Command");
            DBGMSG("    > Ignored / aborted");
            return;
            DBGMSG("OUT part");
            DBGMSG(((Framework.String)"Toggle OUT ") + ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).toggle);
#endif
            CommandBlockWrapper* cbw =
                (CommandBlockWrapper*)
                    Heap.AllocZeroed((uint)sizeof(CommandBlockWrapper),
                        "MassStorageDevice : SendSCSI_StartStopUnitCommand (1)");
            bool FreeStatusBuffer = false;
            try
            {
                SetupSCSICommand(0x1B, cbw, 0, 0);
                cbw->commandByte[1] = (byte)(ImmediateResponse ? 0x1 : 0);
                cbw->commandByte[4] = (byte)(((byte)PowerCondition << 4) | (LoadEject ? 0x2 : 0) | (Start ? 0x1 : 0));

#if MSD_TRACE
                DBGMSG("Setup transfer...");
#endif
                USBTransfer transfer = new USBTransfer();
                DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_OUTEndpointID,
                    512);
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
                        statusBuffer = Heap.AllocZeroed(13u, "MassStorageDevice : SendSCSI_StartStopUnitCommand (2)");
                    }

#if MSD_TRACE
                    DBGMSG("Setup transfer...");
#endif
                    DeviceInfo.hc.SetupTransfer(DeviceInfo, transfer, USBTransferType.Bulk, DeviceInfo.MSD_INEndpointID,
                        512);
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

                    if (!transfer.success || CheckSCSICommand(statusBuffer, 0x1B) != 0)
                    {
                        // TODO: Handle failure/timeout
#if MSD_TRACE
                        DBGMSG("SCSI StartSop Unit (10) (In) command failed!");
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
            }
            finally
            {
                Heap.Free(cbw);
                if (FreeStatusBuffer)
                {
                    Heap.Free(statusBuffer);
                }
            }
        }

        /// <summary>
        ///     The maximum number of times to resend the "ready" query. See TestDeviceReady().
        /// </summary>
        /// <see cref="TestDeviceReady()" />
        public static byte MaxReadyTests = 100;

        /// <summary>
        ///     Tests the device is ready to send/receive data.
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

                bool doBreak = true;
                byte* statusBuffer = (byte*)Heap.AllocZeroed(13u, "MassStorageDevice : TestDeviceReady (1)");
                try
                {
                    //Get the device status by sending the Test Unit Ready command
                    SendSCSICommand_IN(0x00, 0u, 0, null, statusBuffer, false);

                    // Get the status byte that tells us whether the device is ready or not.
                    byte statusByteTestReady = *(statusBuffer + 12);

                    // If we have tested more than half the intended times and the device is still 
                    //  reporting not ready, we just skip the full test to speed up testing a bit.
                    //  (Status byte == 0 inidicates ready).
                    if (statusByteTestReady != 0) continue;

#if MSD_TRACE
                    DBGMSG("SCSI: request sense");
#endif

                    byte* dataBuffer = (byte*)Heap.AllocZeroed(18u, "MassStorageDevice : TestDeviceReady (2)");
                    try
                    {
                        // Now send the Request Sense command
                        SendSCSICommand_IN(0x03, 0, 18, dataBuffer, statusBuffer, false);

                        statusByte = *(statusBuffer + 12);

                        // Get the request sense. 
                        int sense = GetRequestSense(dataBuffer);
                        //  Request sense 0 = No sense = OK
                        //  Request sense 6 = Unit Attention = OK
                        if (sense == 0 || sense == 6)
                        {
                            doBreak = true;
                        }
                    }
                    finally
                    {
                        Heap.Free(dataBuffer);
                    }
                }
                finally
                {
                    Heap.Free(statusBuffer);
                }

                if (doBreak)
                {
                    timeout = 0;
                    break;
                }
                SystemCalls.SleepThread(50);
            }

            return statusByte;
        }

        /// <summary>
        ///     Analyses the inquiry data to extract device-specific information.
        /// </summary>
        /// <param name="inquiryData">A pointer to the data to analyse.</param>
        /// <see cref="!:http://en.wikipedia.org/wiki/SCSI_Inquiry_Command" />
        private static void AnalyseInquiry(byte* inquiryData)
        {
            //TODO: Store this data (inc. #region stuff) in the class for later

            // cf. Jan Axelson, USB Mass Storage, page 140
            byte PeripheralDeviceType = Utils.GetField(inquiryData, 0, 0, 5); // byte, shift, len
            // byte PeripheralQualifier  = Utils.GetField(inquiryData, 0, 5, 3);
            // byte DeviceTypeModifier   = Utils.GetField(inquiryData, 1, 0, 7);
            byte RMB = Utils.GetField(inquiryData, 1, 7, 1);
#if MSD_TRACE
            byte ANSIapprovedVersion = Utils.GetField(inquiryData, 2, 0, 3);
#endif
            // byte ECMAversion          = Utils.GetField(inquiryData, 2, 3, 3);
            // byte ISOversion           = Utils.GetField(inquiryData, 2, 6, 2);
            byte ResponseDataFormat = Utils.GetField(inquiryData, 3, 0, 4);
            byte HISUP = Utils.GetField(inquiryData, 3, 4, 1);
            byte NORMACA = Utils.GetField(inquiryData, 3, 5, 1);
            // byte AdditionalLength     = Utils.GetField(inquiryData, 4, 0, 8);
            byte CmdQue = Utils.GetField(inquiryData, 7, 1, 1);
            byte Linked = Utils.GetField(inquiryData, 7, 3, 1);

#if MSD_TRACE || DEVICE_INFO
            BasicConsole.WriteLine("Vendor ID  : " + Framework.ByteConverter.GetASCIIStringFromASCII(inquiryData, 8, 8));
            BasicConsole.WriteLine("Product ID : " + Framework.ByteConverter.GetASCIIStringFromASCII(inquiryData, 16, 16));

            DBGMSG("Revision   : " + Framework.ByteConverter.GetASCIIStringFromASCII(inquiryData, 1024, 4));

            // Book of Jan Axelson, "USB Mass Storage", page 140:
            // printf("\nVersion ANSI: %u  ECMA: %u  ISO: %u", ANSIapprovedVersion, ECMAversion, ISOversion);
            DBGMSG(((Framework.String)"Version: ") + ANSIapprovedVersion + " (4: SPC-2, 5: SPC-3)");

            // Jan Axelson, USB Mass Storage, page 140
            if (ResponseDataFormat == 2)
            {
                BasicConsole.WriteLine("Response Data Format OK");
            }
            else
            {
                BasicConsole.WriteLine(((Framework.String)"Response Data Format is not OK: ") + ResponseDataFormat + " (should be 2)");
            }

            BasicConsole.WriteLine(((Framework.String)"Removable device type:            ") + (RMB != 0 ? "yes" : "no"));
            BasicConsole.WriteLine(((Framework.String)"Supports hierarch. addr. support: ") + (HISUP != 0 ? "yes" : "no"));
            BasicConsole.WriteLine(((Framework.String)"Supports normal ACA bit support:  ") + (NORMACA != 0 ? "yes" : "no"));
            BasicConsole.WriteLine(((Framework.String)"Supports linked commands:         ") + (Linked != 0 ? "yes" : "no"));
            BasicConsole.WriteLine(((Framework.String)"Supports tagged command queuing:  ") + (CmdQue != 0 ? "yes" : "no"));

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

//            uint* capacityBuffer = (uint*)Framework.Heap.AllocZeroed(8);
//            SendSCSICommand_IN(0x25 /*SCSI opcode*/, 0 /*LBA*/, 8 /*Bytes In*/, capacityBuffer, null);

//            // MSB ... LSB
//            capacityBuffer[0] = Utils.htonl(capacityBuffer[0]);
//            capacityBuffer[1] = Utils.htonl(capacityBuffer[1]);

//            //diskDevice.SetBlockCount(((ulong)capacityBuffer[0]) + 1);
//            //diskDevice.SetBlockSize((ulong)capacityBuffer[1]);

//#if MSD_TRACE
//            DBGMSG(((Framework.String)"Capacity: ") + (diskDevice.BlockCount * diskDevice.BlockSize) + ", Last LBA: " + capacityBuffer[0] + ", block size: " + capacityBuffer[1]);
//#endif
//        }

        /// <summary>
        ///     Reads the specified block of the device.
        /// </summary>
        /// <param name="sector">The sector index to read.</param>
        /// <param name="buffer">The buffer to store the sector data in. Must be at least as big as the sector size.</param>
        /// <returns>True if the read was successful. Otherwise false.</returns>
        public bool Read(uint sector, uint numSectors, void* buffer)
        {
#if MSD_TRACE
            DBGMSG(((Framework.String)">SCSI: read sector: ") + sector);
#endif
            int retries = 3;
            //Send SCSI Read (10) command
            while (!SendSCSICommand_IN(0x28, sector, (ushort)((uint)diskDevice.BlockSize*numSectors), buffer, null) &&
                   --retries > 0)
            {
                ;
            }
            SystemCalls.SleepThread(20);

            return retries > 0;
        }

        /// <summary>
        ///     Writes the specified block of the device.
        /// </summary>
        /// <param name="sector">The sector index to write.</param>
        /// <param name="buffer">The buffer storing the data to write. Must be at least as big as the sector size.</param>
        /// <returns>True if the write was successful. Otherwise false.</returns>
        public bool Write(uint sector, void* buffer)
        {
#if MSD_TRACE
            DBGMSG(((Framework.String)">SCSI: write sector: ") + sector);
#endif

            //Send SCSI Write (10) command
            SendSCSICommand_OUT(0x2A, sector, (ushort)diskDevice.BlockSize, buffer, null);

            return true;
        }

        /// <summary>
        ///     Performs the reset-recovery operation on the specified interface.
        /// </summary>
        public void ResetRecoveryMSD()
        {
            // Start with correct endpoint toggles and reset interface
            ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_OUTEndpointID]).Toggle = false;
            ((Endpoint)DeviceInfo.Endpoints[DeviceInfo.MSD_INEndpointID]).Toggle = false;
            // Reset Interface
            BulkReset(DeviceInfo.MSD_InterfaceNum);

            SystemCalls.SleepThread(500);

            // Clear Feature HALT to the Bulk-In  endpoint
#if MSD_TRACE
            DBGMSG(((Framework.String)"GetStatus: ") + USBManager.GetStatus(DeviceInfo, DeviceInfo.MSD_INEndpointID));
#endif
            USBManager.ClearFeatureHALT(DeviceInfo, DeviceInfo.MSD_INEndpointID);
#if MSD_TRACE
            DBGMSG(((Framework.String)"GetStatus: ") + USBManager.GetStatus(DeviceInfo, DeviceInfo.MSD_INEndpointID));
#endif

            // Clear Feature HALT to the Bulk-Out endpoint
#if MSD_TRACE
            DBGMSG(((Framework.String)"GetStatus: ") + USBManager.GetStatus(DeviceInfo, DeviceInfo.MSD_OUTEndpointID));
#endif
            USBManager.ClearFeatureHALT(DeviceInfo, DeviceInfo.MSD_OUTEndpointID);
#if MSD_TRACE
            DBGMSG(((Framework.String)"GetStatus: ") + USBManager.GetStatus(DeviceInfo, DeviceInfo.MSD_OUTEndpointID));
#endif

            // set configuration to 1 and endpoint IN/OUT toggles to 0
            USBManager.SetConfiguration(DeviceInfo, 1); // set first configuration
            byte config = USBManager.GetConfiguration(DeviceInfo);
#if MSD_TRACE
            if (config != 1)
            {
                DBGMSG(((Framework.String)"Configuration: ") + config + " (to be: 1)");
            }
#endif
            SystemCalls.SleepThread(500);
        }

        /// <summary>
        ///     Human readable descriptions of the SCSI command sense keys.
        /// </summary>
        public static String[] SenseDescriptions =
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
        ///     Gets the request's result's sense.
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
                DBGMSG("Validity:");
                if (Valid == 0)
                {
                    DBGMSG("     > Sense data are not SCSI compliant");
                }
                else
                {
                    DBGMSG("     > Sense data are SCSI compliant");
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
        private static void DBGMSG(Framework.String msg)
        {
            BasicConsole.WriteLine(msg);
        }
#endif
    }

    /// <summary>
    ///     Represents a mass storage device as a disk device. Allows an MSD to be accessed
    ///     like a normal disk device.
    /// </summary>
    public unsafe class MassStorageDevice_DiskDevice : DiskDevice
    {
        /// <summary>
        ///     The MSD that the disk device is an interface for.
        /// </summary>
        protected MassStorageDevice msd;

        /// <summary>
        ///     Initialises a new disk device interface for the specified MSD.
        ///     Also determines the device's maximum size.
        /// </summary>
        /// <param name="anMSD">The MSD to create an interface for.</param>
        public MassStorageDevice_DiskDevice(MassStorageDevice anMSD)
            : base(
                DeviceGroup.Storage, DeviceClass.Storage, DeviceSubClass.USB, "USB Mass Storage Disk", new uint[0], true
                )
        {
            msd = anMSD;

            uint* capacityBuffer = (uint*)Heap.AllocZeroed(8, "MassStorageDevice : MassStorageDevice_DiskDevice()");
            try
            {
                //Send SCSI Read Capacity (10) command
                anMSD.SendSCSICommand_IN(0x25, 0, 8, capacityBuffer, null);

                // MSB ... LSB converted to LSB...MSB
                //  i.e. big-endian converted to little endian
                capacityBuffer[0] = Utils.htonl(capacityBuffer[0]);
                capacityBuffer[1] = Utils.htonl(capacityBuffer[1]);

                // capacityBuffer[0] = Last LBA i.e. last addressable LBA
                //      So the count of blocks is Last LBA + 1
                Blocks = (ulong)capacityBuffer[0] + 1;
                // capacityBuffer[1] = Block size
                blockSize = capacityBuffer[1];
            }
            finally
            {
                Heap.Free(capacityBuffer);
            }
        }

        /// <summary>
        ///     See base class.
        /// </summary>
        /// <param name="BlockNo">See base class.</param>
        /// <param name="BlockCount">See base class.</param>
        /// <param name="Data">See base class.</param>
        public override void ReadBlock(ulong BlockNo, uint BlockCount, byte[] Data)
        {
#if MSD_TRACE
            BasicConsole.WriteLine("Beginning reading...");
#endif

            msd.Activate();

            byte* dataPtr = (byte*)ObjectUtilities.GetHandle(Data) + Array.FieldsBytesSize;
#if MSD_TRACE
            BasicConsole.Write(((Framework.String)"Reading block: ") + i);
#endif

            if (!msd.Read((uint)BlockNo, BlockCount, dataPtr))
            {
                ExceptionMethods.Throw(new Exception("Could not read from Mass Storage Device!"));
            }
            dataPtr += BlockSize;

#if MSD_TRACE
            BasicConsole.WriteLine(" - Read.");
#endif

            msd.Idle(false);

#if MSD_TRACE
            BasicConsole.WriteLine("Completed all reading.");
#endif
        }

        /// <summary>
        ///     See base class.
        /// </summary>
        /// <param name="BlockNo">See base class.</param>
        /// <param name="BlockCount">See base class.</param>
        /// <param name="Data">See base class.</param>
        public override void WriteBlock(ulong BlockNo, uint BlockCount, byte[] Data)
        {
            msd.Activate();

            if (Data == null)
            {
                byte* dataPtr = (byte*)ObjectUtilities.GetHandle(NewBlockArray(1)) + Array.FieldsBytesSize;
                for (uint i = 0; i < BlockCount; i++)
                {
                    msd.Write((uint)(BlockNo + i), dataPtr);
                }
            }
            else
            {
                byte* dataPtr = (byte*)ObjectUtilities.GetHandle(Data) + Array.FieldsBytesSize;
                for (uint i = 0; i < BlockCount; i++)
                {
                    msd.Write((uint)(BlockNo + i), dataPtr);
                    dataPtr += BlockSize;
                }
            }

            msd.Idle(false);
        }

        /// <summary>
        ///     Tests writing and reading from the device.
        /// </summary>
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
                    writeData[i] = (byte)(i%255);
                }
                for (int i = 0; i < dataSize/512; i++)
                {
                    BasicConsole.WriteLine("Writing data...");
                    WriteBlock((uint)i, 1, null);
                }

                BasicConsole.WriteLine("Syncing caches...");
                msd.SendSCSI_SyncCacheCommand(false, false, null);

                byte[] readData = new byte[512];
                bool DataOK = true;
                for (int i = 0; i < dataSize/512; i++)
                {
                    BasicConsole.WriteLine("Reading data...");
                    ReadBlock((uint)i, 1, readData);
                    BasicConsole.WriteLine("Checking data...");

                    for (int j = 0; j < 512; j++)
                    {
                        DataOK = readData[j] == 0; //(j % 255));
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

                    if (!DataOK)
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
        ///     Makes sure all data is written to disk and not stored in hardware caches.
        /// </summary>
        public override void CleanCaches()
        {
            msd.CleanCaches();
        }

        /// <summary>
        ///     Destroys the MSD disk device.
        /// </summary>
        public void Destroy()
        {
            //TODO: This should be done through a DeviceManager.Deregister system call.
            //TODO: This needs un-commenting and fixing
            //DeviceManager.Devices.Remove(this);
            msd = null;
        }
    }

    /// <summary>
    ///     The command block wrapper structure which makes up a SCSI command.
    /// </summary>
    public unsafe struct CommandBlockWrapper
    {
        /// <summary>
        ///     Read the spec.
        /// </summary>
        public uint CBWSignature; // Offset: 0

        /// <summary>
        ///     Read the spec.
        /// </summary>
        public uint CBWTag; // Offset: 4

        /// <summary>
        ///     Read the spec.
        /// </summary>
        public uint CBWDataTransferLength; // Offset: 8

        /// <summary>
        ///     Read the spec.
        /// </summary>
        public byte CBWFlags; // Offset: 12

        /// <summary>
        ///     Read the spec.
        /// </summary>
        public byte CBWLUN; // only bits 3:0, Offset: 13

        /// <summary>
        ///     Read the spec.
        /// </summary>
        public byte CBWCBLength; // only bits 4:0: Offset: 14

        /// <summary>
        ///     Read the spec.
        /// </summary>
        public fixed byte commandByte [16]; // Offset: 15
    }
}