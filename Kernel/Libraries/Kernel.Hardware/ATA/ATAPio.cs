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
    public class ATAPio : ATA
    {
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
        /// Dvc Sel values.
        /// </summary>
        [Flags]
        enum DvcSelVal : byte
        {
            // Bits 0-3: Head Number for CHS.
            // Bit 4: Slave Bit. (0: Selecting Master Drive, 1: Selecting Slave Drive).
            /// <summary>
            /// Slave value.
            /// </summary>
            Slave = 0x10,
            //* Bit 6: LBA (0: CHS, 1: LBA).
            /// <summary>
            /// LBA value.
            /// </summary>
            LBA = 0x40,
            //* Bit 5: Obsolete and isn't used, but should be set.
            //* Bit 7: Obsolete and isn't used, but should be set. 
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
        /// <summary>
        /// Specification levels
        /// </summary>
        public enum SpecLevel
        {
            /// <summary>
            /// Null
            /// </summary>
            Null,
            /// <summary>
            /// ATA
            /// </summary>
            ATA,
            /// <summary>
            /// ATAPI
            /// </summary>
            ATAPI
        }

        /// <summary>
        /// IO ports for this device.
        /// </summary>
        protected ATAIO IO;

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

        /// <summary>
        /// Initialises a new ATA pio device.
        /// </summary>
        /// <param name="anIO">The IO ports for the new Pio device.</param>
        /// <param name="aControllerId">The controller ID for the new device.</param>
        /// <param name="aBusPosition">The bus position of the new device.</param>
        public ATAPio(ATAIO anIO, ATA.ControllerID aControllerId, ATA.BusPosition aBusPosition)
        {
            IO = anIO;
            controllerId = aControllerId;
            busPosition = aBusPosition;
            // Disable IRQs, we use polling currently
            IO.Control.Write_Byte((byte)0x02);

            mDriveType = DiscoverDrive();
            if (mDriveType != SpecLevel.Null)
            {
                InitDrive();
            }
        }

        /// <summary>
        /// Sends the drive select command.
        /// </summary>
        /// <param name="aLbaHigh4">LBA High 4 bits</param>
        public void SelectDrive(byte aLbaHigh4)
        {
            IO.DeviceSelect.Write_Byte((byte)((byte)(DvcSelVal.Default | DvcSelVal.LBA | (busPosition == BusPosition.Slave ? DvcSelVal.Slave : 0)) | aLbaHigh4));
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
            byte xVoid;
            xVoid = IO.Status.Read_Byte();
            xVoid = IO.Status.Read_Byte();
            xVoid = IO.Status.Read_Byte();
            xVoid = IO.Status.Read_Byte();
        }
        /// <summary>
        /// Attempts to discover the ATA drive.
        /// </summary>
        /// <returns>The specification level of the discovered drive. SpecLevel.Null if not found.</returns>
        public SpecLevel DiscoverDrive()
        {
            SelectDrive(0);
            var xIdentifyStatus = SendCmd(Cmd.Identify, false);
            // No drive found, go to next
            if (xIdentifyStatus == Status.None)
            {
                return SpecLevel.Null;
            }
            else if ((xIdentifyStatus & Status.Error) != 0)
            {
                // Can look in Error port for more info
                // Device is not ATA
                // This is also triggered by ATAPI devices
                int xTypeId = IO.LBA2.Read_Byte() << 8 | IO.LBA1.Read_Byte();
                if (xTypeId == 0xEB14 || xTypeId == 0x9669)
                {
                    return SpecLevel.ATAPI;
                }
                else
                {
                    // Unknown type. Might not be a device.
                    return SpecLevel.Null;
                }
            }
            else if ((xIdentifyStatus & Status.DRQ) == 0)
            {
                // Error
                return SpecLevel.Null;
            }
            return SpecLevel.ATA;
        }
        /// <summary>
        /// Attempts to initialise the ATA drive.
        /// </summary>
        protected void InitDrive()
        {
            if (mDriveType == SpecLevel.ATA)
            {
                SendCmd(Cmd.Identify);
            }
            else
            {
                SendCmd(Cmd.IdentifyPacket);
            }
            //IDENTIFY command
            // Not sure if all this is needed, its different than documented elsewhere but might not be bad
            // to add code to do all listed here:
            //
            //To use the IDENTIFY command, select a target drive by sending 0xA0 for the master drive, or 0xB0 for the slave, to the "drive select" IO port. On the Primary bus, this would be port 0x1F6. 
            // Then set the Sectorcount, LBAlo, LBAmid, and LBAhi IO ports to 0 (port 0x1F2 to 0x1F5). 
            // Then send the IDENTIFY command (0xEC) to the Command IO port (0x1F7). 
            // Then read the Status port (0x1F7) again. If the value read is 0, the drive does not exist. For any other value: poll the Status port (0x1F7) until bit 7 (BSY, value = 0x80) clears. 
            // Because of some ATAPI drives that do not follow spec, at this point you need to check the LBAmid and LBAhi ports (0x1F4 and 0x1F5) to see if they are non-zero. 
            // If so, the drive is not ATA, and you should stop polling. Otherwise, continue polling one of the Status ports until bit 3 (DRQ, value = 8) sets, or until bit 0 (ERR, value = 1) sets.
            // At that point, if ERR is clear, the data is ready to read from the Data port (0x1F0). Read 256 words, and store them. 

            // Read Identification Space of the Device
            var xBuff = new UInt16[256];
            IO.Data.Read_UInt16s(xBuff);
            mSerialNo = GetString(xBuff, 10, 20);
            mFirmwareRev = GetString(xBuff, 23, 8);
            mModelNo = GetString(xBuff, 27, 40);

            //Words (61:60) shall contain the value one greater than the total number of user-addressable
            //sectors in 28-bit addressing and shall not exceed 0FFFFFFFh.  The content of words (61:60) shall
            //be greater than or equal to one and less than or equal to 268,435,455.
            // We need 28 bit addressing - small drives on VMWare and possibly other cases are 28 bit
            blockCount = ((UInt32)xBuff[61] << 16 | xBuff[60]) - 1;

            //Words (103:100) shall contain the value one greater than the total number of user-addressable
            //sectors in 48-bit addressing and shall not exceed 0000FFFFFFFFFFFFh.
            //The contents of words (61:60) and (103:100) shall not be used to determine if 48-bit addressing is
            //supported. IDENTIFY DEVICE bit 10 word 83 indicates support for 48-bit addressing.
            bool xLba48Capable = (xBuff[83] & 0x400) != 0;
            if (xLba48Capable)
            {
                blockCount = ((UInt64)xBuff[102] << 32 | (UInt64)xBuff[101] << 16 | (UInt64)xBuff[100]) - 1;
            }
        }

        /// <summary>
        /// Gets a string from the specified UInt16. Equivalent of ASCII byte array conversion.
        /// </summary>
        /// <param name="aBuffer">The data to convert.</param>
        /// <param name="anIndexStart">The index to start converting at.</param>
        /// <param name="aStringLength">The length of the string to create.</param>
        /// <returns>The new string.</returns>
        protected FOS_System.String GetString(UInt16[] aBuffer, int anIndexStart, int aStringLength)
        {
            FOS_System.String newStr = FOS_System.String.New(aStringLength);
            for (int i = 0; i < aStringLength / 2; i++)
            {
                UInt16 xChar = aBuffer[anIndexStart + i];
                newStr[i * 2] = (char)(xChar >> 8);
                newStr[i * 2 + 1] = (char)xChar;
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
            do
            {
                Wait();
                xStatus = (Status)IO.Status.Read_Byte();
            } while ((xStatus & Status.Busy) != 0);

            // Error occurred
            if (aThrowOnError && (xStatus & Status.Error) != 0)
            {
                // TODO: Read error port
                ExceptionMethods.Throw(new FOS_System.Exception("ATA Read port error!"));
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
            SelectDrive((byte)(aSectorNo >> 24));

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

        public override void CleanCaches()
        {
            //Nothing to do for our implementation (thus far...)
        }
    }
}
