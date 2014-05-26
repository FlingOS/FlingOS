using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.ATA
{
    public class ATAPio : ATA
    {
        [Flags]
        public enum Status : byte
        {
            None = 0x00,
            Busy = 0x80,
            ATA_SR_DRD = 0x40,
            ATA_SR_DF = 0x20,
            ATA_SR_DSC = 0x10,
            DRQ = 0x08,
            ATA_SR_COR = 0x04,
            ATA_SR_IDX = 0x02,
            Error = 0x01
        };
        [Flags]
        enum Error : byte
        {
            ATA_ER_BBK = 0x80,
            ATA_ER_UNC = 0x40,
            ATA_ER_MC = 0x20,
            ATA_ER_IDNF = 0x10,
            ATA_ER_MCR = 0x08,
            ATA_ER_ABRT = 0x04,
            ATA_ER_TK0NF = 0x02,
            ATA_ER_AMNF = 0x01
        };
        [Flags]
        enum DvcSelVal : byte
        {
            // Bits 0-3: Head Number for CHS.
            // Bit 4: Slave Bit. (0: Selecting Master Drive, 1: Selecting Slave Drive).
            Slave = 0x10,
            //* Bit 6: LBA (0: CHS, 1: LBA).
            LBA = 0x40,
            //* Bit 5: Obsolete and isn't used, but should be set.
            //* Bit 7: Obsolete and isn't used, but should be set. 
            Default = 0xA0
        };
        public enum Cmd : byte
        {
            ReadPio = 0x20,
            ReadPioExt = 0x24,
            ReadDma = 0xC8,
            ReadDmaExt = 0x25,
            WritePio = 0x30,
            WritePioExt = 0x34,
            WriteDma = 0xCA,
            WriteDmaExt = 0x35,
            CacheFlush = 0xE7,
            CacheFlushExt = 0xEA,
            Packet = 0xA0,
            IdentifyPacket = 0xA1,
            Identify = 0xEC,
            Read = 0xA8,
            Eject = 0x1B
        }
        public enum Ident : byte
        {
            DEVICETYPE = 0,
            CYLINDERS = 2,
            HEADS = 6,
            SECTORS = 12,
            SERIAL = 20,
            MODEL = 54,
            CAPABILITIES = 98,
            FIELDVALID = 106,
            MAX_LBA = 120,
            COMMANDSETS = 164,
            MAX_LBA_EXT = 200
        }
        public enum SpecLevel
        {
            Null,
            ATA,
            ATAPI
        }

        protected ATAIO IO;

        protected SpecLevel mDriveType = SpecLevel.Null;
        public SpecLevel DriveType
        {
            get { return mDriveType; }
        }

        protected FOS_System.String mSerialNo;
        public FOS_System.String SerialNo
        {
            get { return mSerialNo; }
        }

        protected FOS_System.String mFirmwareRev;
        public FOS_System.String FirmwareRev
        {
            get { return mFirmwareRev; }
        }

        protected FOS_System.String mModelNo;
        public FOS_System.String ModelNo
        {
            get { return mModelNo; }
        }

        public ATAPio(ATAIO anIO, ATA.ControllerId aControllerId, ATA.BusPosition aBusPosition)
        {
            IO = anIO;
            controllerId = aControllerId;
            busPosition = aBusPosition;
            // Disable IRQs, we use polling currently
            IO.Control.Write((byte)0x02);

            mDriveType = DiscoverDrive();
            if (mDriveType != SpecLevel.Null)
            {
                InitDrive();
            }
        }

        public void SelectDrive(byte aLbaHigh4)
        {
            IO.DeviceSelect.Write((byte)((byte)(DvcSelVal.Default | DvcSelVal.LBA | (busPosition == BusPosition.Slave ? DvcSelVal.Slave : 0)) | aLbaHigh4));
            Wait();
        }
        // ATA requires a wait of 400 nanoseconds.
        // Read the Status register FIVE TIMES, and only pay attention to the value 
        // returned by the last one -- after selecting a new master or slave device. The point being that 
        // you can assume an IO port read takes approximately 100ns, so doing the first four creates a 400ns 
        // delay -- which allows the drive time to push the correct voltages onto the bus. 
        // Since we read status again later, we wait by reading it 4 times.
        protected void Wait()
        {
            // Wait 400 ns
            byte xVoid;
            xVoid = IO.Status.Read_Byte();
            xVoid = IO.Status.Read_Byte();
            xVoid = IO.Status.Read_Byte();
            xVoid = IO.Status.Read_Byte();
        }
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
            IO.Data.Read16(xBuff);
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

        public Status SendCmd(Cmd aCmd)
        {
            return SendCmd(aCmd, true);
        }
        public Status SendCmd(Cmd aCmd, bool aThrowOnError)
        {
            IO.Command.Write((byte)aCmd);
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
        
        protected void SelectSector(UInt64 aSectorNo, UInt32 aSectorCount)
        {
            //TODO: Check for 48 bit sectorno mode and select 48 bits
            SelectDrive((byte)(aSectorNo >> 24));

            // Number of sectors to read
            IO.SectorCount.Write((byte)aSectorCount);
            IO.LBA0.Write((byte)(aSectorNo & 0xFF));
            IO.LBA1.Write((byte)((aSectorNo & 0xFF00) >> 8));
            IO.LBA2.Write((byte)((aSectorNo & 0xFF0000) >> 16));
            //TODO LBA3  ...
        }
        public override void ReadBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData)
        {
            SelectSector(aBlockNo, aBlockCount);
            SendCmd(Cmd.ReadPio);
            IO.Data.Read8(aData);
        }
        public override void WriteBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData)
        {
            SelectSector(aBlockNo, aBlockCount);
            SendCmd(Cmd.WritePio);

            if (aData == null)
            {
                ulong size = (aBlockCount / 2) * blockSize;
                for (ulong i = 0; i < size; i++)
                {
                    IO.Data.Write(0);
                }
            }
            else
            {
                UInt16 xValue;

                for (int i = 0; i < aData.Length / 2; i++)
                {
                    xValue = (UInt16)((aData[i * 2 + 1] << 8) | aData[i * 2]);
                    IO.Data.Write(xValue);
                }
            }

            SendCmd(Cmd.CacheFlush);
        }
    }
}
