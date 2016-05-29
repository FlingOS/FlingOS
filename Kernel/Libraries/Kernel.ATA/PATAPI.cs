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

//#define PATAPI_TRACE

using Kernel.ATA.Exceptions;
using Kernel.Devices;
using Kernel.Framework;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Devices;

namespace Kernel.ATA
{
    /// <summary>
    ///     Driver for handling PATAPI disks.
    /// </summary>
    public class PATAPI : DiskDevice
    {
        /// <summary>
        ///     Whether IRQ 14 has been invoked or not.
        /// </summary>
        private static bool IRQ14Invoked;

        /// <summary>
        ///     Whether IRQ 15 has been invoked or not.
        /// </summary>
        private static bool IRQ15Invoked;

        /// <summary>
        ///     The underlying PATA device that this PATAPI driver is wrapping.
        /// </summary>
        private readonly PATABase BaseDevice;

        /// <summary>
        ///     The serial number of the device.
        /// </summary>
        /// <seealso cref="PATABase.SerialNo" />
        public String SerialNo => BaseDevice.SerialNo;

        /// <summary>
        ///     The firmware revision of the device.
        /// </summary>
        public String FirmwareRev => BaseDevice.FirmwareRev;

        /// <summary>
        ///     The model number of the device.
        /// </summary>
        public String ModelNo => BaseDevice.ModelNo;

        /// <summary>
        ///     The total number of logical blocks on the currently inserted disc. Undefined if no disc is present.
        /// </summary>
        public override ulong Blocks => BaseDevice.Blocks;

        /// <summary>
        ///     The size (in bytes) of the logical blocks on the currently inserted disc. Undefined if no disc is present.
        /// </summary>
        public override ulong BlockSize => BaseDevice.BlockSize;

        /// <summary>
        ///     The maximum number of logical blocks to write in a single PIO command for the drive.
        /// </summary>
        /// <remarks>
        ///     This limit is necessary because some real-world drives do not conform to standards.
        /// </remarks>
        public uint MaxWritePioBlocks => BaseDevice.MaxWritePioBlocks;

        /// <summary>
        ///     True if the relevant IRQ for the device has been invoked. Otherwise, false.
        /// </summary>
        private bool IRQInvoked
        {
            get { return BaseDevice.ControllerId == ATA.ControllerIds.Primary ? IRQ14Invoked : IRQ15Invoked; }
            set
            {
                if (BaseDevice.ControllerId == ATA.ControllerIds.Primary)
                {
                    IRQ14Invoked = value;
                }
                else
                {
                    IRQ15Invoked = value;
                }
            }
        }

        /// <summary>
        ///     Initialises a new PATAPI driver for the specified device.
        /// </summary>
        /// <param name="BaseDevice">The PATAPI device to be wrapped.</param>
        internal PATAPI(PATABase BaseDevice)
            : base(DeviceGroup.Storage, DeviceClass.Storage, DeviceSubClass.ATA, "PATAPI Disk", BaseDevice.Info, true)
        {
            this.BaseDevice = BaseDevice;

            // Enable IRQs - required for PATAPI
            this.BaseDevice.SelectDrive(0, false);
            this.BaseDevice.IO.Control.Write_Byte(0x00);

            //Note: IRQHandler is hooked from DeviceManagerTask
            //TODO: Delegate for the handler should be passed up to Device Manager from this class
        }

        /// <summary>
        ///     Handles IRQs by checking if they are relevant and setting the correct IRQ Invoked flag if necessary.
        /// </summary>
        /// <seealso cref="IRQInvoked" />
        /// <param name="IRQNumber">The number of the IRQ that has triggered the interrupt.</param>
        public static void IRQHandler(uint IRQNumber)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (IRQNumber)
            {
                case 14:
                    IRQ14Invoked = true;
                    break;
                case 15:
                    IRQ15Invoked = true;
                    break;
            }
        }

        /// <summary>
        ///     Waits up to 100ms for an IRQ to occur.
        /// </summary>
        /// <returns>True if an IRQ was invoked during the wait, otherwise false.</returns>
        private bool WaitForIRQ()
        {
            int Timeout = 20;
            while (!IRQInvoked && Timeout-- > 0)
                SystemCalls.SleepThread(5);

            return IRQInvoked;
        }

        /// <summary>
        ///     Reads contiguous logical blocks from the device.
        /// </summary>
        /// <param name="BlockNo">The logical block number to read.</param>
        /// <param name="BlockCount">The number of blocks to read.</param>
        /// <param name="Data">The byte array to store the data in.</param>
        public override void ReadBlock(ulong BlockNo, uint BlockCount, byte[] Data)
        {
            // Reset IRQ (by reading status register)
#if PATAPI_TRACE
            BasicConsole.WriteLine("Reset IRQ");
#endif
            BaseDevice.IO.Status.Read_Byte();
            IRQInvoked = false;

            // Select the drive
#if PATAPI_TRACE
            BasicConsole.WriteLine("Select drive");
#endif
            BaseDevice.SelectDrive(0, false);

            // Read the data
            for (uint i = 0; i < BlockCount; i++)
            {
#if PATAPI_TRACE
                BasicConsole.WriteLine("Read block");
#endif
                _ReadBlock(BlockNo + i, Data, (uint)(i*BlockSize));
            }
        }

        /// <summary>
        ///     Reads a single block from position BlockNo on the disc into the Data array at the specified Offset.
        /// </summary>
        /// <param name="BlockNo">The number of the block to read.</param>
        /// <param name="Data">The array to store the data in.</param>
        /// <param name="Offset">The offset in the Data array to start storing the data at.</param>
        private void _ReadBlock(ulong BlockNo, byte[] Data, uint Offset)
        {
            // Setup the packet
#if PATAPI_TRACE
            BasicConsole.WriteLine("Setup ATAPI packet");
#endif
            byte[] PATAPIPacket = new byte[12];
            PATAPIPacket[0] = 0xA8;
            PATAPIPacket[1] = 0x0;
            PATAPIPacket[2] = (byte)(BlockNo >> 24);
            PATAPIPacket[3] = (byte)(BlockNo >> 16);
            PATAPIPacket[4] = (byte)(BlockNo >> 8);
            PATAPIPacket[5] = (byte)(BlockNo >> 0);
            PATAPIPacket[6] = 0x0;
            PATAPIPacket[7] = 0x0;
            PATAPIPacket[8] = 0x0;
            PATAPIPacket[9] = 1;
            PATAPIPacket[10] = 0x0;
            PATAPIPacket[11] = 0x0;

            // Inform the controller we are using PIO mode
#if PATAPI_TRACE
            BasicConsole.WriteLine("Tell controller we are using PIO mode");
#endif
            BaseDevice.IO.Features.Write_Byte(0);

            // Tell the drive the buffer size
#if PATAPI_TRACE
            BasicConsole.WriteLine("Tell drive the buffer size");
#endif
            BaseDevice.IO.LBA1.Write_Byte((byte)BlockSize); // Low byte
            BaseDevice.IO.LBA1.Write_Byte((byte)(BlockSize >> 8)); // High byte

            // Send the packet command (includes the wait)
#if PATAPI_TRACE
            BasicConsole.WriteLine("Send Packet command");
#endif
            PATABase.Status Status = BaseDevice.SendCmd(PATABase.Cmd.Packet);

            // Error occurred
#if PATAPI_TRACE
            BasicConsole.WriteLine("Check for error");
#endif
            if ((Status & PATABase.Status.Error) != 0)
            {
#if PATAPI_TRACE
                BasicConsole.WriteLine("Error detected");
#endif
                ExceptionMethods.Throw(new NoDiskException("ATAPI read error! Status bits incorrect in first check."));
            }

            // Check if that invoked an IRQ - it shouldn't have
#if PATAPI_TRACE
            BasicConsole.WriteLine("Check if IRQ invoked");
#endif
            if (IRQInvoked)
            {
#if PATAPI_TRACE
                BasicConsole.WriteLine("IRQ had been invoked");
#endif
                // Allow future IRQs by reading Status register
                BaseDevice.IO.Status.Read_Byte();
                IRQInvoked = false;
            }

            // Send the data
#if PATAPI_TRACE
            BasicConsole.WriteLine("Write packet data");
#endif
            BaseDevice.IO.Data.Write_UInt16s(PATAPIPacket);

            // Wait a bit
#if PATAPI_TRACE
            BasicConsole.WriteLine("Brief wait");
#endif
            BaseDevice.Wait();

            // Wait for the IRQ
#if PATAPI_TRACE
            BasicConsole.WriteLine("Wait for IRQ");
#endif
            if (!WaitForIRQ())
            {
#if PATAPI_TRACE
                BasicConsole.WriteLine("Error! Wait for IRQ timed out.");
                BasicConsole.DelayOutput(5);
#endif
            }

            // Wait for Busy to clear and check alternate status
#if PATAPI_TRACE
            BasicConsole.WriteLine("Wait till not busy");
#endif
            uint Timeout = 0xF0000000;
            do
            {
                BaseDevice.Wait();
                Status = (PATABase.Status)BaseDevice.IO.Control.Read_Byte();
            } while ((Status & PATABase.Status.Busy) != 0 &&
                     (Status & PATABase.Status.Error) == 0 &&
                     Timeout-- > 0);

            // Read status reg to clear IRQ
#if PATAPI_TRACE
            BasicConsole.WriteLine("Read status");
#endif
            Status = (PATABase.Status)BaseDevice.IO.Status.Read_Byte();
            IRQInvoked = false;

            // Error occurred
#if PATAPI_TRACE
            BasicConsole.WriteLine("Check for error");
#endif
            if ((Status & (PATABase.Status.Error | PATABase.Status.ATA_SR_DF)) != 0 ||
                (Status & PATABase.Status.DRQ) == 0)
            {
#if PATAPI_TRACE
                BasicConsole.WriteLine("Error detected");
#endif
                ExceptionMethods.Throw(new NoDiskException("ATAPI read error! Status bits incorrect in second check."));
            }

            // Read the data
#if PATAPI_TRACE
            BasicConsole.WriteLine("Read the data");
            BasicConsole.WriteLine("Length: " + (Framework.String)Data.Length);
#endif
            uint CurrentOffset = Offset + 1;
            uint i = 0;
            for (; i < BlockSize && CurrentOffset < Data.Length; i += 2, CurrentOffset += 2)
            {
                ushort Value = BaseDevice.IO.Data.Read_UInt16();
                Data[CurrentOffset - 1] = (byte)Value;
                Data[CurrentOffset] = (byte)(Value >> 8);
            }
            // Clear out any remaining data
            for (; i < BlockSize; i++)
            {
                BaseDevice.IO.Data.Read_UInt16();
            }

#if PATAPI_TRACE
            unsafe
            {
                BasicConsole.DumpMemory((byte*)Utilities.ObjectUtilities.GetHandle(Data) + Framework.Array.FieldsBytesSize, Data.Length);
            }

            BasicConsole.WriteLine("Wait for IRQ");
#endif
            // Wait for IRQ
            if (!WaitForIRQ())
            {
#if PATAPI_TRACE
                BasicConsole.WriteLine("Error! Wait for IRQ timed out. (1)");
                BasicConsole.DelayOutput(5);
#endif
            }

            // Wait for Busy and DRQ to clear and check status
#if PATAPI_TRACE
            BasicConsole.WriteLine("Wait till not busy");
#endif
            Timeout = 0xF0000000;
            do
            {
                BaseDevice.Wait();
                Status = (PATABase.Status)BaseDevice.IO.Control.Read_Byte();
            } while ((Status & (PATABase.Status.Busy | PATABase.Status.DRQ)) != 0 &&
                     (Status & PATABase.Status.Error) == 0 &&
                     Timeout-- > 0);

            // Error occurred
#if PATAPI_TRACE
            BasicConsole.WriteLine("Check for error");
#endif
            if ((Status & (PATABase.Status.Error | PATABase.Status.ATA_SR_DF)) != 0 ||
                (Status & PATABase.Status.DRQ) != 0)
            {
#if PATAPI_TRACE
                BasicConsole.WriteLine("Error detected");
#endif
                ExceptionMethods.Throw(new Exception("ATAPI read error! Status bits incorrect in third check."));
            }

#if PATAPI_TRACE
            BasicConsole.WriteLine("Complete");
            BasicConsole.DelayOutput(10);
#endif
        }

        //TODO: Implement PATAPI.WriteBlock
        /// <summary>
        ///     Writing to PATAPI drives is not supported yet. This method will throw a <see cref="NotSupportedException" />
        /// </summary>
        /// <param name="BlockNo">Unused</param>
        /// <param name="BlockCount">Unused</param>
        /// <param name="Data">Unused</param>
        public override void WriteBlock(ulong BlockNo, uint BlockCount, byte[] Data)
            => ExceptionMethods.Throw(new NotSupportedException("Cannot write to PATAPI device!"));

        /// <summary>
        ///     Writing to PATAPI devices is currently not supported so this method will do nothing. No exception is thrown.
        /// </summary>
        public override void CleanCaches()
        {
            //TODO: Implement PATAPI.CleanCaches when PATAPI.WriteBlock is implemented
        }


        /*
         * This is PATAPI related:
         *
         *      TODO: Suspect this should be being used for the Identity command

                /// <summary>
                /// Identity values.
                /// </summary>
                public enum Ident : byte
                {
                    /// <summary>
                    /// Device type
                    /// </summary>
                    DEVICETYPE = 0,
                    /// <summary>
                    /// Cylinders
                    /// </summary>
                    CYLINDERS = 2,
                    /// <summary>
                    /// Heads
                    /// </summary>
                    HEADS = 6,
                    /// <summary>
                    /// Sectors
                    /// </summary>
                    SECTORS = 12,
                    /// <summary>
                    /// Serial
                    /// </summary>
                    SERIAL = 20,
                    /// <summary>
                    /// Model
                    /// </summary>
                    MODEL = 54,
                    /// <summary>
                    /// Capabilities
                    /// </summary>
                    CAPABILITIES = 98,
                    /// <summary>
                    /// Field valid
                    /// </summary>
                    FIELDVALID = 106,
                    /// <summary>
                    /// Max LBA
                    /// </summary>
                    MAX_LBA = 120,
                    /// <summary>
                    /// Command sets
                    /// </summary>
                    COMMANDSETS = 164,
                    /// <summary>
                    /// Max LBA extended
                    /// </summary>
                    MAX_LBA_EXT = 200
                }

        */
    }
}