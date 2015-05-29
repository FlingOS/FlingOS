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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.ATA
{
    /// <summary>
    /// Represents an ATA Pio device.
    /// </summary>
    public class PATA : Devices.DiskDevice
    {
        //TODO - This implementation does not support LBA48 mode.

        protected PATABase BaseDevice;

        public FOS_System.String SerialNo
        {
            get { return BaseDevice.SerialNo; }
        }
        public FOS_System.String FirmwareRev
        {
            get { return BaseDevice.FirmwareRev; }
        }
        public FOS_System.String ModelNo
        {
            get { return BaseDevice.ModelNo; }
        }

        /// <summary>
        /// Initialises a new ATA pio device.
        /// </summary>
        /// <param name="anIO">The IO ports for the new Pio device.</param>
        /// <param name="aControllerId">The controller ID for the new device.</param>
        /// <param name="aBusPosition">The bus position of the new device.</param>
        public PATA(PATABase baseDevice)
        {
            BaseDevice = baseDevice;

            blockSize = BaseDevice.BlockSize;
        }
                
        /// <summary>
        /// Selects the specified contiguous sectors on the drive.
        /// </summary>
        /// <param name="aSectorNo">The first sector to select.</param>
        /// <param name="aSectorCount">The number of contiguous sectors to select.</param>
        protected void SelectSector(UInt64 aSectorNo, UInt32 aSectorCount)
        {
            //TODO: Check for 48 bit sectorno mode and select 48 bits
            BaseDevice.SelectDrive((byte)(aSectorNo >> 24), true);

            // Number of sectors to read
            BaseDevice.IO.SectorCount.Write_Byte((byte)aSectorCount);
            BaseDevice.IO.LBA0.Write_Byte((byte)(aSectorNo & 0xFF));
            BaseDevice.IO.LBA1.Write_Byte((byte)((aSectorNo & 0xFF00) >> 8));
            BaseDevice.IO.LBA2.Write_Byte((byte)((aSectorNo & 0xFF0000) >> 16));
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
            if (!BaseDevice.initialised)
            {
                return;
            }

            SelectSector(aBlockNo, aBlockCount);
            BaseDevice.SendCmd(PATABase.Cmd.ReadPio);
            BaseDevice.IO.Data.Read_Bytes(aData);
        }
        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="aBlockNo">See base class.</param>
        /// <param name="aBlockCount">See base class.</param>
        /// <param name="aData">See base class.</param>
        public override void WriteBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData)
        {
            if (!BaseDevice.initialised)
            {
                return;
            }

            SelectSector(aBlockNo, aBlockCount);
            BaseDevice.SendCmd(PATABase.Cmd.WritePio);

            if (aData == null)
            {
                //TODO: Remove the cast-down - only due to division of longs not working...
                ulong size = (aBlockCount * (uint)blockSize) / 2;
                for (ulong i = 0; i < size; i++)
                {
                    BaseDevice.IO.Data.Write_UInt16(0);
                }
            }
            else
            {
                UInt16 xValue;

                for (int i = 0; i < aData.Length / 2; i++)
                {
                    xValue = (UInt16)((aData[i * 2 + 1] << 8) | aData[i * 2]);
                    BaseDevice.IO.Data.Write_UInt16(xValue);
                }
            }

            BaseDevice.SendCmd(PATABase.Cmd.CacheFlush);
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