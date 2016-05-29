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

using System;
using System.Diagnostics.CodeAnalysis;
using Exception = Kernel.Framework.Exception;
using NotSupportedException = Kernel.Framework.Exceptions.NotSupportedException;
using String = Kernel.Framework.String;

namespace Kernel.ATA
{
    /// <summary>
    ///     Represents any generic PATA device.
    ///     PATA Base Device provides a common set of data and functions for managing PATA devices.
    ///     The PATA and PATAPI classes wrap the base device with a specific implementation.
    /// </summary>
    /// <remarks>
    ///     The base device is also used to initially detect devices and carry out the basic identification protocol.
    ///     This allows the driver to determine the actual type of disk and thus create the correct wrapper for the
    ///     device.
    /// </remarks>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal sealed class PATABase : ATA
    {
        /// <summary>
        ///     ATA Pio commands.
        /// </summary>
        public enum Cmd : byte
        {
            /// <summary>
            ///     Read Pio command.
            /// </summary>
            ReadPIO = 0x20,

            /// <summary>
            ///     Read Pio extended command.
            /// </summary>
            ReadPIOExt = 0x24,

            /// <summary>
            ///     Read direct memory access command.
            /// </summary>
            ReadDMA = 0xC8,

            /// <summary>
            ///     Read direct memory access extended command.
            /// </summary>
            ReadDMAExt = 0x25,

            /// <summary>
            ///     Write Pio command.
            /// </summary>
            WritePIO = 0x30,

            /// <summary>
            ///     Write Pio extended command.
            /// </summary>
            WritePIOExt = 0x34,

            /// <summary>
            ///     Write direct memory access command.
            /// </summary>
            WriteDMA = 0xCA,

            /// <summary>
            ///     Write direct memory access extended command.
            /// </summary>
            WriteDMAExt = 0x35,

            /// <summary>
            ///     Cache flush command.
            /// </summary>
            CacheFlush = 0xE7,

            /// <summary>
            ///     Cache flush extended command.
            /// </summary>
            CacheFlushExt = 0xEA,

            /// <summary>
            ///     Packet command.
            /// </summary>
            Packet = 0xA0,

            /// <summary>
            ///     Identify packet command.
            /// </summary>
            /// <remarks>
            ///     Unused. This is for PATAPI devices only.
            /// </remarks>
            IdentifyPacket = 0xA1,

            /// <summary>
            ///     Identify command.
            /// </summary>
            Identify = 0xEC,

            /// <summary>
            ///     Read command.
            /// </summary>
            Read = 0xA8,

            /// <summary>
            ///     Eject command.
            /// </summary>
            Eject = 0x1B
        }

        /// <summary>
        ///     Drive Select values.
        /// </summary>
        [Flags]
        public enum DriveSelectValue : byte
        {
            // Bits 0-3: Head Number for CHS.
            // Bit 4: Slave Bit. (0: Selecting Master Drive, 1: Selecting Slave Drive).
            /// <summary>
            ///     Slave value.
            /// </summary>
            Slave = 0x10,
            // Bit 6: LBA (0: CHS, 1: LBA).
            /// <summary>
            ///     LBA value.
            /// </summary>
            LBA = 0x40,
            // Bit 5: Obsolete and isn't used, but should be set.
            // Bit 7: Obsolete and isn't used, but should be set. 
            /// <summary>
            ///     Default value.
            /// </summary>
            Default = 0xA0
        }

        /// <summary>
        ///     Error masks.
        /// </summary>
        [Flags]
        public enum Error : byte
        {
            /// <summary>
            ///     BBK error.
            /// </summary>
            ATA_ER_BBK = 0x80,

            /// <summary>
            ///     UNC error.
            /// </summary>
            ATA_ER_UNC = 0x40,

            /// <summary>
            ///     MC error.
            /// </summary>
            ATA_ER_MC = 0x20,

            /// <summary>
            ///     IDNF error.
            /// </summary>
            ATA_ER_IDNF = 0x10,

            /// <summary>
            ///     MCR error.
            /// </summary>
            ATA_ER_MCR = 0x08,

            /// <summary>
            ///     ABRT error.
            /// </summary>
            ATA_ER_ABRT = 0x04,

            /// <summary>
            ///     TK0NF error.
            /// </summary>
            ATA_ER_TK0NF = 0x02,

            /// <summary>
            ///     AMNF error.
            /// </summary>
            ATA_ER_AMNF = 0x01
        }

        /// <summary>
        ///     Specification levels (Drive types and identifiers)
        /// </summary>
        public enum SpecLevel : ushort
        {
            /// <summary>
            ///     Null
            /// </summary>
            /// <remarks>
            ///     This value is arbitary and has no meaning in the ATA spec.
            /// </remarks>
            Null = 0x0000,

            /// <summary>
            ///     PATA
            /// </summary>
            /// <remarks>
            ///     This value is arbitary and has no meaning in the ATA spec.
            /// </remarks>
            PATA = 0x0001,

            /// <summary>
            ///     SATA
            /// </summary>
            /// <remarks>
            ///     This value is NOT arbitary. It is the SATA device identifier.
            /// </remarks>
            SATA = 0xC33C,

            /// <summary>
            ///     PATAPI
            /// </summary>
            /// <remarks>
            ///     This value is NOT arbitary. It is the PATAPI device identifier.
            /// </remarks>
            PATAPI = 0xEB14,

            /// <summary>
            ///     SATAPI
            /// </summary>
            /// <remarks>
            ///     This value is NOT arbitary. It is the SATAPI device identifier.
            /// </remarks>
            SATAPI = 0x9669
        }

        /// <summary>
        ///     Device statuses.
        /// </summary>
        [Flags]
        public enum Status : byte
        {
            /// <summary>
            ///     No status.
            /// </summary>
            None = 0x00,

            /// <summary>
            ///     Busy status.
            /// </summary>
            Busy = 0x80,

            /// <summary>
            ///     ATA_SR_DRD status.
            /// </summary>
            ATA_SR_DRD = 0x40,

            /// <summary>
            ///     ATA_SR_DF status.
            /// </summary>
            ATA_SR_DF = 0x20,

            /// <summary>
            ///     ATA_SR_DSC status.
            /// </summary>
            ATA_SR_DSC = 0x10,

            /// <summary>
            ///     DRQ status.
            /// </summary>
            DRQ = 0x08,

            /// <summary>
            ///     ATA_SR_COR status.
            /// </summary>
            ATA_SR_COR = 0x04,

            /// <summary>
            ///     ATA_SR_IDX status.
            /// </summary>
            ATA_SR_IDX = 0x02,

            /// <summary>
            ///     Error status.
            /// </summary>
            Error = 0x01,

            /// <summary>
            ///     Timeout error.
            /// </summary>
            Timeout = 0xFF
        }

        /// <summary>
        ///     IO ports for this device.
        /// </summary>
        public readonly ATAIOPorts IO;

        /// <summary>
        ///     PIO drive type.
        /// </summary>
        public SpecLevel DriveType { get; }

        /// <summary>
        ///     Drive's serial number.
        /// </summary>
        public String SerialNo { get; private set; }

        /// <summary>
        ///     Drive's firmware reivision.
        /// </summary>
        public String FirmwareRev { get; private set; }

        /// <summary>
        ///     Drive's model number.
        /// </summary>
        public String ModelNo { get; private set; }

        /// <summary>
        ///     Whether the drive is capable of LBA-48 Mode commands.
        /// </summary>
        public bool LBA48Mode { get; private set; }

        /// <summary>
        ///     Whether the drive has been initialised or not.
        /// </summary>
        public bool Initialised { get; private set; }

        /// <summary>
        ///     The maximum number of logical blocks to write in a single PIO command for the drive.
        /// </summary>
        /// <remarks>
        ///     This limit is necessary because some real-world drives do not conform to standards.
        /// </remarks>
        public uint MaxWritePioBlocks { get; private set; } = 128;

        /// <summary>
        ///     Initialises a new PATA Base device with the specified information.
        /// </summary>
        /// <param name="IO">The IO Ports to use for accessing the device.</param>
        /// <param name="ControllerId">The controller identifier of the device.</param>
        /// <param name="BusPosition">The position of the device on the ATA bus.</param>
        public PATABase(ATAIOPorts IO, ControllerIds ControllerId, BusPositions BusPosition)
            : base("PATA Base Device")
        {
            this.IO = IO;
            Info[0] = (uint)(this.ControllerId = ControllerId);
            Info[1] = (uint)(this.BusPosition = BusPosition);

            // Disable IRQs, we use polling currently
            SelectDrive(0, false);
            this.IO.Control.Write_Byte(0x02);

            DriveType = DiscoverDrive();

            if (DriveType == SpecLevel.PATAPI)
            {
                blockSize = 2048;
            }

            if (DriveType == SpecLevel.PATA ||
                DriveType == SpecLevel.PATAPI)
            {
                InitDrive();
                Initialised = true;
            }
        }

        /// <summary>
        ///     Attempts to initialise the ATA drive.
        /// </summary>
        private void InitDrive()
        {
            // At this point, DiscoverDrive has been called, but the additional identification data 
            // has not been read

            // If it's a PATAPI drive, we need to send the IDENTIFY_PACKET command
            if (DriveType == SpecLevel.PATAPI)
            {
                Reset();
                SendCmd(Cmd.IdentifyPacket);
            }

            // Read Identification Space of the Device
            var DeviceInfoBuffer = new ushort[256];
            IO.Data.Read_UInt16s(DeviceInfoBuffer);
            SerialNo = GetString(DeviceInfoBuffer, 10, 20).Trim();
            FirmwareRev = GetString(DeviceInfoBuffer, 23, 8).Trim();
            ModelNo = GetString(DeviceInfoBuffer, 27, 40).Trim();

            // Hitachi hardrives found in real-world hardware failed in that:
            //      They only work with one-sector writes at a time
            //  This may be due to the fact that I'm working with old laptops and a sample size of one
            //  Hitachi drive. Newer drives may work properly. All drives will work with a max size of 
            //  one though.
            //  
            //  Fujitsu drives suffer a similar fault.
            if (ModelNo.StartsWith("Hitachi") || ModelNo.StartsWith("FUJITSU"))
            {
                MaxWritePioBlocks = 1;
            }

            //Words (61:60) shall contain the value one greater than the total number of user-addressable
            //sectors in 28-bit addressing and shall not exceed 0x0FFFFFFF. 
            // We need 28 bit addressing - small drives on VMWare and possibly other cases are 28 bit
            Blocks = ((uint)DeviceInfoBuffer[61] << 16 | DeviceInfoBuffer[60]) - 1;

            //Words (103:100) shall contain the value one greater than the total number of user-addressable
            //sectors in 48-bit addressing and shall not exceed 0x0000FFFFFFFFFFFF.
            //The contents of words (61:60) and (103:100) shall not be used to determine if 48-bit addressing is
            //supported. IDENTIFY DEVICE bit 10 word 83 indicates support for 48-bit addressing.
            bool LBA48Capable = (DeviceInfoBuffer[83] & 0x400) != 0;
            if (LBA48Capable)
            {
                Blocks = ((ulong)DeviceInfoBuffer[103] << 48 | (ulong)DeviceInfoBuffer[102] << 32 |
                          (ulong)DeviceInfoBuffer[101] << 16 | DeviceInfoBuffer[100]) - 1;
                LBA48Mode = true;
            }
        }

        /// <summary>
        ///     Gets a string from the specified UInt16[]. Equivalent of ASCII byte array conversion.
        /// </summary>
        /// <param name="Buffer">The data to convert.</param>
        /// <param name="StartIndex">The index to start converting at.</param>
        /// <param name="StrLength">The length of the string to create.</param>
        /// <returns>The new string.</returns>
        private static String GetString(ushort[] Buffer, int StartIndex, int StrLength)
        {
            //Each UInt16 is treated as 2 ASCII characters.
            //  The upshot is that the Buffer is essentially a byte array 
            //  and we are treating each byte as one ASCII character.

            String NewString = String.New(StrLength);
            for (int i = 0; i < StrLength/2; i++)
            {
                ushort CurrentChar = Buffer[StartIndex + i];
                NewString[i*2] = (char)((CurrentChar >> 8) & 0xFF);
                NewString[i*2 + 1] = (char)(CurrentChar & 0xFF);
            }
            return NewString;
        }


        /// <summary>
        ///     Sends the drive select command and waits for 400ns.
        /// </summary>
        /// <param name="LBAHigh4Bits">LBA High 4 bits (also specify SetLBA)</param>
        /// <param name="SetLBA">Whether to set the LBA flag and high 4 bits or not.</param>
        internal void SelectDrive(byte LBAHigh4Bits, bool SetLBA)
        {
            if (SetLBA)
            {
                IO.DeviceSelect.Write_Byte(
                    (byte)
                        ((byte)
                            (DriveSelectValue.Default | DriveSelectValue.LBA |
                             (BusPosition == BusPositions.Slave ? DriveSelectValue.Slave : 0)) | LBAHigh4Bits));
            }
            else
            {
                IO.DeviceSelect.Write_Byte(
                    (byte)
                        ((byte)
                            (DriveSelectValue.Default | (BusPosition == BusPositions.Slave ? DriveSelectValue.Slave : 0)) |
                         LBAHigh4Bits));
            }
            Wait();
        }

        /// <summary>
        ///     Waits by performing 4 reads (see remarks / ATA spec)
        /// </summary>
        /// <remarks>
        ///     ATA requires a wait of 400 nanoseconds.
        ///     Read the Status register FIVE TIMES, and only pay attention to the value
        ///     returned by the last one -- after selecting a new master or slave device. The point being that
        ///     you can assume an IO port read takes approximately 100ns, so doing the first four creates a 400ns
        ///     delay -- which allows the drive time to push the correct voltages onto the bus.
        ///     Since we read status again later, we wait by reading it 4 times.
        /// </remarks>
        internal void Wait()
        {
            // Wait 400 ns (by reading alternate status register)
            //   Note: The alternate status register does not affect interrupts.
            IO.Control.Read_Byte();
            IO.Control.Read_Byte();
            IO.Control.Read_Byte();
            IO.Control.Read_Byte();
        }

        /// <summary>
        ///     Attempts to discover the ATA drive.
        /// </summary>
        /// <returns>The specification level of the discovered drive. SpecLevel.Null if not found.</returns>
        internal SpecLevel DiscoverDrive()
        {
            SelectDrive(0, false);
            IO.SectorCount.Write_Byte(0);
            IO.LBA0.Write_Byte(0);
            IO.LBA1.Write_Byte(0);
            IO.LBA2.Write_Byte(0);
            Status TheStatus = SendCmd(Cmd.Identify, false);

            if ((TheStatus & Status.Error) != 0)
            {
                // Can look in Error port for more info
                // Device is not ATA
                // Error status can also triggered by ATAPI devices
                // So check LBA1 and LBA2 to detect an ATAPI device.
                ushort TypeId = (ushort)(IO.LBA2.Read_Byte() << 8 | IO.LBA1.Read_Byte());
                switch (TypeId)
                {
                    case (ushort)SpecLevel.PATAPI:
                        return SpecLevel.PATAPI;
                    case (ushort)SpecLevel.SATAPI:
                        return SpecLevel.SATAPI;
                    case (ushort)SpecLevel.SATA:
                        return SpecLevel.SATA;
                    default:
                        // Unknown type. Might not be a device.
                        return SpecLevel.Null;
                }
            }

            // No drive found, go to next
            if (TheStatus == Status.None)
            {
                return SpecLevel.Null;
            }

            // To handle some ATAPI devices that do not conform to spec 
            // (i.e. they do not throw an error response to the Device Select command),
            //  check LBA1 and LBA2 ports for non-zero values.
            //  If they are non-zero, then the drive is not ATA.
            {
                ushort TypeId = (ushort)(IO.LBA2.Read_Byte() << 8 | IO.LBA1.Read_Byte());
                // It is, however, possible to detect what type of device is actually attached.
                switch (TypeId)
                {
                    case 0:
                        // PATA - Proceed as normal
                        break;
                    case (ushort)SpecLevel.PATAPI:
                        return SpecLevel.PATAPI;
                    case (ushort)SpecLevel.SATAPI:
                        return SpecLevel.SATAPI;
                    case (ushort)SpecLevel.SATA:
                        return SpecLevel.SATA;
                    default:
                        // Unknown type. Might not be a device.
                        return SpecLevel.Null;
                }
            }

            do
            {
                Wait();
                TheStatus = (Status)IO.Status.Read_Byte();
            } while ((TheStatus & Status.DRQ) == 0 &&
                     (TheStatus & Status.Error) == 0);

            if ((TheStatus & Status.Error) != 0)
            {
                // Error
                return SpecLevel.Null;
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if ((TheStatus & Status.DRQ) == 0)
            {
                // Error
                return SpecLevel.Null;
            }

            return SpecLevel.PATA;
        }

        /// <summary>
        ///     Sends the specified command (with ThrowOnError=true).
        /// </summary>
        /// <param name="TheCommand">The command to send.</param>
        /// <returns>The device status.</returns>
        internal Status SendCmd(Cmd TheCommand) => SendCmd(TheCommand, true);

        /// <summary>
        ///     Sends the specified command.
        /// </summary>
        /// <param name="TheCommand">The command to send.</param>
        /// <param name="ThrowOnError">
        ///     Whether to throw an exception if the device reports
        ///     an error status.
        /// </param>
        /// <returns>The device status.</returns>
        internal Status SendCmd(Cmd TheCommand, bool ThrowOnError)
        {
            IO.Command.Write_Byte((byte)TheCommand);
            Status TheStatus;
            int Timeout = 20000000;
            do
            {
                Wait();
                TheStatus = (Status)IO.Control.Read_Byte();
            } while ((TheStatus & Status.Busy) != 0 &&
                     (TheStatus & Status.Error) == 0 &&
                     Timeout-- > 0);

            // Error occurred
            if (ThrowOnError && ((TheStatus & Status.Error) != 0 || Timeout == 0))
            {
                #region Throw Exception 

                String CommandName = "";
                switch (TheCommand)
                {
                    case Cmd.CacheFlush:
                        CommandName = "CacheFlush";
                        break;
                    case Cmd.CacheFlushExt:
                        CommandName = "CacheFlushExt";
                        break;
                    case Cmd.Eject:
                        CommandName = "Eject";
                        break;
                    case Cmd.Identify:
                        CommandName = "Identify";
                        break;
                    case Cmd.IdentifyPacket:
                        CommandName = "IdentifyPacket";
                        break;
                    case Cmd.Packet:
                        CommandName = "Packet";
                        break;
                    case Cmd.Read:
                        CommandName = "Read";
                        break;
                    case Cmd.ReadDMA:
                        CommandName = "ReadDMA";
                        break;
                    case Cmd.ReadDMAExt:
                        CommandName = "ReadDMAExt";
                        break;
                    case Cmd.ReadPIO:
                        CommandName = "ReadPIO";
                        break;
                    case Cmd.ReadPIOExt:
                        CommandName = "ReadPIOExt";
                        break;
                    case Cmd.WriteDMA:
                        CommandName = "WriteDMA";
                        break;
                    case Cmd.WriteDMAExt:
                        CommandName = "WriteDMAExt";
                        break;
                    case Cmd.WritePIO:
                        CommandName = "WritePIO";
                        break;
                    case Cmd.WritePIOExt:
                        CommandName = "WritePIOExt";
                        break;
                    default:
                        CommandName = "[Unrecognised]";
                        break;
                }
                ExceptionMethods.Throw(new Exception("ATA send command error! Command was: " + CommandName));

                #endregion
            }
            return Timeout == 0 ? Status.Timeout : TheStatus;
        }

        /// <summary>
        ///     Resets the device.
        /// </summary>
        public void Reset()
        {
            IO.Control.Write_Byte(0x4);
            Wait();
            IO.Control.Write_Byte(0x0);
            Status TheStatus;
            int Timeout = 20000000;
            do
            {
                Wait();
                TheStatus = (Status)IO.Control.Read_Byte();
            } while ((TheStatus & Status.Busy) != 0 &&
                     (TheStatus & Status.Error) == 0 &&
                     Timeout-- > 0);

            // Error occurred
            if ((TheStatus & Status.Error) != 0)
            {
                ExceptionMethods.Throw(new Exception("ATA software reset error!"));
            }
            else if (Timeout == 0)
            {
                ExceptionMethods.Throw(new Exception("ATA software reset timeout!"));
            }

            //Reselect the correct drive
            SelectDrive(0, false);
        }

        /// <summary>
        ///     Cannot read or write an unspecific ATA device! Use one of the specific driver classes: PATA/PATAPI/SATA/SATAPI.
        ///     Throws Not Supported Exception.
        /// </summary>
        /// <param name="BlockNo">Unused</param>
        /// <param name="BlockCount">Unused</param>
        /// <param name="Data">Unused</param>
        public override void ReadBlock(ulong BlockNo, uint BlockCount, byte[] Data)
            => ExceptionMethods.Throw(new NotSupportedException("Read from PATABase object not supported!"));

        /// <summary>
        ///     Cannot read or write an unspecific ATA device! Use one of the specific driver classes: PATA/PATAPI.
        ///     Throws Not Supported Exception.
        /// </summary>
        /// <param name="BlockNo">Unused</param>
        /// <param name="BlockCount">Unused</param>
        /// <param name="Data">Unused</param>
        public override void WriteBlock(ulong BlockNo, uint BlockCount, byte[] Data)
            => ExceptionMethods.Throw(new NotSupportedException("Write to PATABase object not supported!"));

        /// <summary>
        ///     Cannot write an unspecific ATA device and therefore cannot clean caches of one either! Use one of the specific
        ///     driver classes: PATA/PATAPI.
        ///     Throws Not Supported Exception.
        /// </summary>
        /// <remarks>
        ///     Unlike in other classes, this method does throw an exception. This is on the basis that a PATABase device should
        ///     never be passed outside of
        ///     an ATA driver (or manager) - it is internal. Thus there should be no high level management code that is able to
        ///     directly call this method.
        ///     If there is such code able to access this class directly, then the class has been misused.
        /// </remarks>
        public override void CleanCaches()
            =>
                ExceptionMethods.Throw(
                    new NotSupportedException("CleanCaches of PATABase object should never be called!"));
    }
}