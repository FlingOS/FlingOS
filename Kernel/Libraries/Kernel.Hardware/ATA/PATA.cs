#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.ATA
{
    /// <summary>
    /// Represents an ATA Pio device.
    /// </summary>
    public class PATA : ATA
    {
        //TODO - This implementation does not support CD drives. I.e. does not support PATAPI devices.
        //  Note: This driver should never support SATA or SATAPI devices. Write a separate driver for that.

        /// <summary>
        /// Device statuses.
        /// </summary>
        [Flags]
        public enum Status : byte
        {
            /// <summary>
            /// No status.
            /// </summary>
            None = 0x00,
            /// <summary>
            /// Busy status.
            /// </summary>
            Busy = 0x80,
            /// <summary>
            /// ATA_SR_DRD status.
            /// </summary>
            ATA_SR_DRD = 0x40,
            /// <summary>
            /// ATA_SR_DF status.
            /// </summary>
            ATA_SR_DF = 0x20,
            /// <summary>
            /// ATA_SR_DSC status.
            /// </summary>
            ATA_SR_DSC = 0x10,
            /// <summary>
            /// DRQ status.
            /// </summary>
            DRQ = 0x08,
            /// <summary>
            /// ATA_SR_COR status.
            /// </summary>
            ATA_SR_COR = 0x04,
            /// <summary>
            /// ATA_SR_IDX status.
            /// </summary>
            ATA_SR_IDX = 0x02,
            /// <summary>
            /// Error status.
            /// </summary>
            Error = 0x01
        };
        /// <summary>
        /// Error masks.
        /// </summary>
        [Flags]
        enum Error : byte
        {
            /// <summary>
            /// BBK error.
            /// </summary>
            ATA_ER_BBK = 0x80,
            /// <summary>
            /// UNC error.
            /// </summary>
            ATA_ER_UNC = 0x40,
            /// <summary>
            /// MC error.
            /// </summary>
            ATA_ER_MC = 0x20,
            /// <summary>
            /// IDNF error.
            /// </summary>
            ATA_ER_IDNF = 0x10,
            /// <summary>
            /// MCR error.
            /// </summary>
            ATA_ER_MCR = 0x08,
            /// <summary>
            /// ABRT error.
            /// </summary>
            ATA_ER_ABRT = 0x04,
            /// <summary>
            /// TK0NF error.
            /// </summary>
            ATA_ER_TK0NF = 0x02,
            /// <summary>
            /// AMNF error.
            /// </summary>
            ATA_ER_AMNF = 0x01
        };
        /// <summary>
        /// Drive Select values.
        /// </summary>
        [Flags]
        enum DriveSelectValue : byte
        {
            // Bits 0-3: Head Number for CHS.
            // Bit 4: Slave Bit. (0: Selecting Master Drive, 1: Selecting Slave Drive).
            /// <summary>
            /// Slave value.
            /// </summary>
            Slave = 0x10,
            // Bit 6: LBA (0: CHS, 1: LBA).
            /// <summary>
            /// LBA value.
            /// </summary>
            LBA = 0x40,
            // Bit 5: Obsolete and isn't used, but should be set.
            // Bit 7: Obsolete and isn't used, but should be set. 
            /// <summary>
            /// Default value.
            /// </summary>
            Default = 0xA0
        };
        /// <summary>
        /// ATA Pio commands.
        /// </summary>
        public enum Cmd : byte
        {
            /// <summary>
            /// Read Pio command.
            /// </summary>
            ReadPio = 0x20,
            /// <summary>
            /// Read Pio extended command.
            /// </summary>
            ReadPioExt = 0x24,
            /// <summary>
            /// Read direct memory access command.
            /// </summary>
            ReadDma = 0xC8,
            /// <summary>
            /// Read direct memory access extended command.
            /// </summary>
            ReadDmaExt = 0x25,
            /// <summary>
            /// Write Pio command.
            /// </summary>
            WritePio = 0x30,
            /// <summary>
            /// Write Pio extended command.
            /// </summary>
            WritePioExt = 0x34,
            /// <summary>
            /// Write direct memory access command.
            /// </summary>
            WriteDma = 0xCA,
            /// <summary>
            /// Write direct memory access extended command.
            /// </summary>
            WriteDmaExt = 0x35,
            /// <summary>
            /// Cache flush command.
            /// </summary>
            CacheFlush = 0xE7,
            /// <summary>
            /// Cache flush extended command.
            /// </summary>
            CacheFlushExt = 0xEA,
            /// <summary>
            /// Packet command.
            /// </summary>
            Packet = 0xA0,
            /// <summary>
            /// Identify packet command.
            /// </summary>
            /// <remarks>
            /// Unused. This is for PATAPI devices only.
            /// </remarks>
            IdentifyPacket = 0xA1,
            /// <summary>
            /// Identify command.
            /// </summary>
            Identify = 0xEC,
            /// <summary>
            /// Read command.
            /// </summary>
            Read = 0xA8,
            /// <summary>
            /// Eject command.
            /// </summary>
            Eject = 0x1B
        }
        /// <summary>
        /// Specification levels (Drive types and identifiers)
        /// </summary>
        public enum SpecLevel : ushort
        {
            /// <summary>
            /// Null
            /// </summary>
            /// <remarks>
            /// This value is arbitary and has no meaning in the ATA spec.
            /// </remarks>
            Null = 0x0000,
            /// <summary>
            /// PATA
            /// </summary>
            /// <remarks>
            /// This value is arbitary and has no meaning in the ATA spec.
            /// </remarks>
            PATA = 0x0001,
            /// <summary>
            /// SATA
            /// </summary>
            /// <remarks>
            /// This value is NOT arbitary. It is the SATA device identifier.
            /// </remarks>
            SATA = 0xC33C,
            /// <summary>
            /// PATAPI
            /// </summary>
            /// <remarks>
            /// This value is NOT arbitary. It is the PATAPI device identifier.
            /// </remarks>
            PATAPI = 0xEB14,
            /// <summary>
            /// SATAPI
            /// </summary>
            /// <remarks>
            /// This value is NOT arbitary. It is the SATAPI device identifier.
            /// </remarks>
            SATAPI = 0x9669
        }

        /// <summary>
        /// IO ports for this device.
        /// </summary>
        protected ATAIOPorts IO;

        /// <summary>
        /// Pio drive type.
        /// </summary>
        protected SpecLevel mDriveType = SpecLevel.Null;
        /// <summary>
        /// Pio drive type.
        /// </summary>
        public SpecLevel DriveType
        {
            get { return mDriveType; }
        }

        /// <summary>
        /// Drive's serial number.
        /// </summary>
        protected FOS_System.String mSerialNo;
        /// <summary>
        /// Drive's .
        /// </summary>
        public FOS_System.String SerialNo
        {
            get { return mSerialNo; }
        }

        /// <summary>
        /// Drive's .
        /// </summary>
        protected FOS_System.String mFirmwareRev;
        /// <summary>
        /// Drive's .
        /// </summary>
        public FOS_System.String FirmwareRev
        {
            get { return mFirmwareRev; }
        }

        /// <summary>
        /// Drive's model number.
        /// </summary>
        protected FOS_System.String mModelNo;
        /// <summary>
        /// Drive's model number.
        /// </summary>
        public FOS_System.String ModelNo
        {
            get { return mModelNo; }
        }

        protected bool mLBA48Mode = false;
        public bool LBA48Mode
        {
            get
            {
                return mLBA48Mode;
            }
        }

        protected bool initialised = false;
        public bool Initialised
        {
            get
            {
                return initialised;
            }
        }

        /// <summary>
        /// Initialises a new ATA pio device.
        /// </summary>
        /// <param name="anIO">The IO ports for the new Pio device.</param>
        /// <param name="aControllerId">The controller ID for the new device.</param>
        /// <param name="aBusPosition">The bus position of the new device.</param>
        public PATA(ATAIOPorts anIO, ATA.ControllerID aControllerId, ATA.BusPosition aBusPosition)
        {
            IO = anIO;
            controllerId = aControllerId;
            busPosition = aBusPosition;
            // Disable IRQs, we use polling currently
            IO.Control.Write_Byte((byte)0x02);

            mDriveType = DiscoverDrive();
            if (mDriveType == SpecLevel.PATA)
            {
                InitDrive();
                initialised = true;
            }
        }

        /// <summary>
        /// Sends the drive select command.
        /// </summary>
        /// <param name="aLbaHigh4">LBA High 4 bits</param>
        public void SelectDrive(byte aLbaHigh4, bool setLBA)
        {
            if (setLBA)
            {
                IO.DeviceSelect.Write_Byte((byte)((byte)(DriveSelectValue.Default | DriveSelectValue.LBA | (busPosition == BusPosition.Slave ? DriveSelectValue.Slave : 0)) | aLbaHigh4));
            }
            else
            {
                IO.DeviceSelect.Write_Byte((byte)((byte)(DriveSelectValue.Default | (busPosition == BusPosition.Slave ? DriveSelectValue.Slave : 0)) | aLbaHigh4));
            }
            Wait();
        }
        
        /// <summary>
        /// Waits by performing 4 reads (see remarks / ATA spec)
        /// </summary>
        /// <remarks>
        /// ATA requires a wait of 400 nanoseconds.
        /// Read the Status register FIVE TIMES, and only pay attention to the value 
        /// returned by the last one -- after selecting a new master or slave device. The point being that 
        /// you can assume an IO port read takes approximately 100ns, so doing the first four creates a 400ns 
        /// delay -- which allows the drive time to push the correct voltages onto the bus. 
        /// Since we read status again later, we wait by reading it 4 times.
        /// </remarks>
        protected void Wait()
        {
            // Wait 400 ns
            IO.Status.Read_Byte();
            IO.Status.Read_Byte();
            IO.Status.Read_Byte();
            IO.Status.Read_Byte();
        }
        /// <summary>
        /// Attempts to discover the ATA drive.
        /// </summary>
        /// <returns>The specification level of the discovered drive. SpecLevel.Null if not found.</returns>
        public SpecLevel DiscoverDrive()
        {
            SelectDrive(0, false);
            IO.SectorCount.Write_Byte(0);
            IO.LBA0.Write_Byte(0);
            IO.LBA1.Write_Byte(0);
            IO.LBA2.Write_Byte(0);
            var status = SendCmd(Cmd.Identify, false);

            if ((status & Status.Error) != 0)
            {
                // Can look in Error port for more info
                // Device is not ATA
                // Error status can also triggered by ATAPI devices
                // So check LBA1 and LBA2 to detect an ATAPI device.
                ushort typeId = (ushort)(IO.LBA2.Read_Byte() << 8 | IO.LBA1.Read_Byte());
                if (typeId == (ushort)SpecLevel.PATAPI)
                {
                    return SpecLevel.PATAPI;
                }
                else if (typeId == (ushort)SpecLevel.SATAPI)
                {
                    return SpecLevel.SATAPI;
                }
                else if (typeId == (ushort)SpecLevel.SATA)
                {
                    return SpecLevel.SATA;
                }
                else
                {
                    // Unknown type. Might not be a device.
                    return SpecLevel.Null;
                }
            }

            // No drive found, go to next
            if (status == Status.None)
            {
                return SpecLevel.Null;
            }

            // To handle some ATAPI devices that do not conform to spec 
            // (i.e. they do not throw an error response to the Device Select command),
            //  check LBA1 and LBA2 ports for non-zero values.
            //  If they are non-zero, then the drive is not ATA.
            {
                ushort typeId = (ushort)(IO.LBA2.Read_Byte() << 8 | IO.LBA1.Read_Byte());
                // It is, however, possible to detect what type of device is actually attached.
                if (typeId == (ushort)SpecLevel.PATAPI)
                {
                    return SpecLevel.PATAPI;
                }
                else if (typeId == (ushort)SpecLevel.SATAPI)
                {
                    return SpecLevel.SATAPI;
                }
                else if (typeId == (ushort)SpecLevel.SATA)
                {
                    return SpecLevel.SATA;
                }
                else if (typeId != 0u)
                {
                    // Unknown type. Might not be a device.
                    return SpecLevel.Null;
                }
            }

            do
            {
                Wait();
                status = (Status)IO.Status.Read_Byte();
            } while ((status & Status.DRQ) == 0 &&
                     (status & Status.Error) == 0);

            if ((status & Status.Error) != 0)
            {
                // Error
                return SpecLevel.Null;
            }

            if ((status & Status.DRQ) == 0)
            {
                // Error
                return SpecLevel.Null;
            }

            return SpecLevel.PATA;
        }
        /// <summary>
        /// Attempts to initialise the ATA drive.
        /// </summary>
        protected void InitDrive()
        {
            // At this point, DiscoverDrive has been called, but the additional identification data 
            // has not been read

            // Read Identification Space of the Device
            var xBuff = new UInt16[256];
            IO.Data.Read_UInt16s(xBuff);
            mSerialNo = GetString(xBuff, 10, 20);
            mFirmwareRev = GetString(xBuff, 23, 8);
            mModelNo = GetString(xBuff, 27, 40);

            //Words (61:60) shall contain the value one greater than the total number of user-addressable
            //sectors in 28-bit addressing and shall not exceed 0x0FFFFFFF. 
            // We need 28 bit addressing - small drives on VMWare and possibly other cases are 28 bit
            blockCount = ((UInt32)xBuff[61] << 16 | xBuff[60]) - 1;

            //Words (103:100) shall contain the value one greater than the total number of user-addressable
            //sectors in 48-bit addressing and shall not exceed 0x0000FFFFFFFFFFFF.
            //The contents of words (61:60) and (103:100) shall not be used to determine if 48-bit addressing is
            //supported. IDENTIFY DEVICE bit 10 word 83 indicates support for 48-bit addressing.
            bool xLba48Capable = (xBuff[83] & 0x400) != 0;
            if (xLba48Capable)
            {
                blockCount = ((UInt64)xBuff[103] << 48 | (UInt64)xBuff[102] << 32 | (UInt64)xBuff[101] << 16 | (UInt64)xBuff[100]) - 1;
                mLBA48Mode = true;
            }
        }

        /// <summary>
        /// Gets a string from the specified UInt16[]. Equivalent of ASCII byte array conversion.
        /// </summary>
        /// <param name="buffer">The data to convert.</param>
        /// <param name="startIndex">The index to start converting at.</param>
        /// <param name="strLength">The length of the string to create.</param>
        /// <returns>The new string.</returns>
        protected FOS_System.String GetString(UInt16[] buffer, int startIndex, int strLength)
        {
            //Each UInt16 is treated as 2 ASCII characters.
            //  The upshot is that the buffer is essentially a byte array 
            //  and we are treating each byte as one ASCII character.

            FOS_System.String newStr = FOS_System.String.New(strLength);
            for (int i = 0; i < strLength / 2; i++)
            {
                UInt16 xChar = buffer[startIndex + i];
                newStr[i * 2] = (char)((xChar >> 8) & 0xFF);
                newStr[i * 2 + 1] = (char)(xChar & 0xFF);
            }
            return newStr;
        }

        /// <summary>
        /// Sends the specified command (with ThrowOnError=true).
        /// </summary>
        /// <param name="aCmd">The command to send.</param>
        /// <returns>The device status.</returns>
        public Status SendCmd(Cmd aCmd)
        {
            return SendCmd(aCmd, true);
        }
        /// <summary>
        /// Sends the specified command.
        /// </summary>
        /// <param name="aCmd">The command to send.</param>
        /// <param name="aThrowOnError">
        /// Whether to throw an exception if the device reports 
        /// an error status.
        /// </param>
        /// <returns>The device status.</returns>
        public Status SendCmd(Cmd aCmd, bool aThrowOnError)
        {
            IO.Command.Write_Byte((byte)aCmd);
            Status xStatus;
            int timeout = 20000000;
            do
            {
                Wait();
                xStatus = (Status)IO.Status.Read_Byte();
            } while ((xStatus & Status.Busy) != 0 &&
                     (xStatus & Status.Error) == 0 && 
                     timeout-- > 0);

            // Error occurred
            if (aThrowOnError && (xStatus & Status.Error) != 0)
            {
                // TODO: ATA - Handle Send Command error
                //ExceptionMethods.Throw(new FOS_System.Exception("ATA Read port error!"));
                BasicConsole.WriteLine("ATA: Send command error unhandled!");
                BasicConsole.DelayOutput(5);
            }
            return xStatus;
        }
        
        /// <summary>
        /// Selects the specified contiguous sectors on the drive.
        /// </summary>
        /// <param name="aSectorNo">The first sector to select.</param>
        /// <param name="aSectorCount">The number of contiguous sectors to select.</param>
        protected void SelectSector(UInt64 aSectorNo, UInt32 aSectorCount)
        {
            //TODO: Check for 48 bit sectorno mode and select 48 bits
            SelectDrive((byte)(aSectorNo >> 24), true);

            // Number of sectors to read
            IO.SectorCount.Write_Byte((byte)aSectorCount);
            IO.LBA0.Write_Byte((byte)(aSectorNo & 0xFF));
            IO.LBA1.Write_Byte((byte)((aSectorNo & 0xFF00) >> 8));
            IO.LBA2.Write_Byte((byte)((aSectorNo & 0xFF0000) >> 16));
            //TODO LBA3  ...
        }
        /// <summary>
        /// Reads contiguous blocks from the drive.
        /// </summary>
        /// <param name="aBlockNo">The number of the first block to read.</param>
        /// <param name="aBlockCount">The number of contiguous blocks to read.</param>
        /// <param name="aData">The data array to read into.</param>
        public override void ReadBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData)
        {
            if (!initialised)
            {
                return;
            }

            SelectSector(aBlockNo, aBlockCount);
            SendCmd(Cmd.ReadPio);
            IO.Data.Read_Bytes(aData);
        }
        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="aBlockNo">See base class.</param>
        /// <param name="aBlockCount">See base class.</param>
        /// <param name="aData">See base class.</param>
        public override void WriteBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData)
        {
            if (!initialised)
            {
                return;
            }

            SelectSector(aBlockNo, aBlockCount);
            SendCmd(Cmd.WritePio);

            if (aData == null)
            {
                //TODO: Remove the cast-down - only due to multiplication of longs not working...
                ulong size = (aBlockCount * (uint)blockSize) / 2;
                for (ulong i = 0; i < size; i++)
                {
                    IO.Data.Write_UInt16(0);
                }
            }
            else
            {
                UInt16 xValue;

                for (int i = 0; i < aData.Length / 2; i++)
                {
                    xValue = (UInt16)((aData[i * 2 + 1] << 8) | aData[i * 2]);
                    IO.Data.Write_UInt16(xValue);
                }
            }

            SendCmd(Cmd.CacheFlush);
        }

        /// <summary>
        /// Cleans the software and hardware caches (if any) by writing cached data to disk 
        /// if necessary before wiping the cache.
        /// </summary>
        public override void CleanCaches()
        {
            //TODO: Presumably Drive Select needs to happen first? But does the sector number 
            //      need to be set?
            //SendCmd(Cmd.CacheFlush);
        }
    }
}

/*
 * This is PATAPI related:
 *
 
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