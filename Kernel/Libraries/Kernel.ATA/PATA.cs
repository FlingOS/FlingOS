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

using Kernel.Devices;
using Kernel.Framework;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes.Requests.Devices;

namespace Kernel.ATA
{
    /// <summary>
    ///     Represents an ATA Pio device.
    /// </summary>
    public sealed class PATA : DiskDevice
    {
        //TODO: This implementation does not support LBA48 mode.

        /// <summary>
        ///     The underlying PATA device that this PATA driver is wrapping.
        /// </summary>
        private readonly PATABase BaseDevice;

        /// <summary>
        ///     The serial number of the device.
        /// </summary>
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
        ///     The total number of logical blocks on the hard disk.
        /// </summary>
        public override ulong Blocks => BaseDevice.Blocks;

        /// <summary>
        ///     The size (in bytes) of the logical blocks on the hard disk.
        /// </summary>
        public override ulong BlockSize => BaseDevice.BlockSize;

        /// <summary>
        ///     The position of the drive on the ATA bus.
        /// </summary>
        public ATA.BusPositions BusPosition => BaseDevice.BusPosition;

        /// <summary>
        ///     The controller identifier of the drive.
        /// </summary>
        public ATA.ControllerIds ControllerIds => BaseDevice.ControllerId;

        /// <summary>
        ///     The maximum number of logical blocks to write in a single PIO command for the drive.
        /// </summary>
        /// <remarks>
        ///     This limit is necessary because some real-world drives do not conform to standards.
        /// </remarks>
        public uint MaxWritePioBlocks => BaseDevice.MaxWritePioBlocks;

        /// <summary>
        ///     Initialises a new ATA pio device.
        /// </summary>
        internal PATA(PATABase BaseDevice)
            : base(DeviceGroup.Storage, DeviceClass.Storage, DeviceSubClass.ATA, "PATA Disk", BaseDevice.Info, true)
        {
            this.BaseDevice = BaseDevice;
        }

        /// <summary>
        ///     Selects the specified contiguous sectors on the drive.
        /// </summary>
        /// <param name="SectorNo">The first sector to select.</param>
        /// <param name="SectorCount">The number of contiguous sectors to select.</param>
        private void SelectSector(ulong SectorNo, uint SectorCount)
        {
            //TODO: Check for 48 bit sectorno mode and select 48 bits
            BaseDevice.SelectDrive((byte)(SectorNo >> 24), true);

            // Number of sectors to read
            BaseDevice.IO.SectorCount.Write_Byte((byte)SectorCount);
            BaseDevice.IO.LBA0.Write_Byte((byte)(SectorNo & 0xFF));
            BaseDevice.IO.LBA1.Write_Byte((byte)((SectorNo & 0xFF00) >> 8));
            BaseDevice.IO.LBA2.Write_Byte((byte)((SectorNo & 0xFF0000) >> 16));
            //TODO: LBA3  ...
        }

        /// <summary>
        ///     Reads contiguous blocks from the drive.
        /// </summary>
        /// <param name="BlockNo">The number of the first block to read.</param>
        /// <param name="BlockCount">The number of contiguous blocks to read.</param>
        /// <param name="Data">The data array to read into.</param>
        public override void ReadBlock(ulong BlockNo, uint BlockCount, byte[] Data)
        {
            if (!BaseDevice.Initialised)
            {
                return;
            }

            SelectSector(BlockNo, BlockCount);
            BaseDevice.SendCmd(PATABase.Cmd.ReadPIO);
            BaseDevice.IO.Data.Read_Bytes(Data);
        }

        /// <summary>
        ///     Writes contiguous logical blocks to the device.
        /// </summary>
        /// <param name="BlockNo">The number of the first block to write.</param>
        /// <param name="BlockCount">The number of blocks to write.</param>
        /// <param name="Data">The data to write. Pass null to efficiently write 0s to the device.</param>
        /// <remarks>
        ///     If data is null, all data to be written should be assumed to be 0.
        /// </remarks>
        public override void WriteBlock(ulong BlockNo, uint BlockCount, byte[] Data)
        {
            if (!BaseDevice.Initialised)
            {
                return;
            }

            if (Data == null)
            {
                for (uint i = 0; i < BlockCount; i += MaxWritePioBlocks)
                {
                    if (i + MaxWritePioBlocks <= BlockCount)
                    {
                        _WriteBlock(BlockNo + i, MaxWritePioBlocks, null);
                    }
                    else
                    {
                        _WriteBlock(BlockNo + i, BlockCount - i, null);
                    }
                }
            }
            else
            {
                int Offset = 0;
                for (uint i = 0; i < BlockCount; i += MaxWritePioBlocks)
                {
                    uint CurrentBlockCount = MaxWritePioBlocks;
                    if (i + MaxWritePioBlocks > BlockCount)
                    {
                        CurrentBlockCount = BlockCount - i;
                    }

                    SelectSector(BlockNo + i, CurrentBlockCount);
                    BaseDevice.SendCmd(PATABase.Cmd.WritePIO);
                    for (int j = 0; j < (int)((uint)BlockSize/2*CurrentBlockCount); j++)
                    {
                        ushort Value = (ushort)((Data[j*2 + 1 + Offset] << 8) | Data[j*2 + Offset]);
                        BaseDevice.IO.Data.Write_UInt16(Value);
                    }
                    BaseDevice.SendCmd(PATABase.Cmd.CacheFlush);

                    Offset += (int)((uint)BlockSize*CurrentBlockCount);
                }
            }
        }

        /// <summary>
        ///     Writes Data for an exact number of blocks to the device.
        /// </summary>
        /// <param name="BlockNo">The block number to start writing at.</param>
        /// <param name="BlockCount">The number of blocks to write.</param>
        /// <param name="Data">THe data to write. The length of the data must be the exact number of blocks.</param>
        private void _WriteBlock(ulong BlockNo, uint BlockCount, byte[] Data)
        {
            SelectSector(BlockNo, BlockCount);
            BaseDevice.SendCmd(PATABase.Cmd.WritePIO);

            if (Data == null)
            {
                //TODO: Remove the cast-down - only due to division of longs not working...
                ulong Size = BlockCount*(uint)BlockSize/2;
                for (ulong i = 0; i < Size; i++)
                {
                    BaseDevice.IO.Data.Write_UInt16(0);
                }
            }
            else
            {
                if (Data.Length != BlockCount*(uint)BlockSize)
                {
                    ExceptionMethods.Throw(
                        new ArgumentException((String)"Data to write is not the correct length! Length:" + Data.Length +
                                              ", Expected: " + BlockCount*(uint)BlockSize));
                }

                for (int i = 0; i < Data.Length/2; i++)
                {
                    ushort Value = (ushort)((Data[i*2 + 1] << 8) | Data[i*2]);
                    BaseDevice.IO.Data.Write_UInt16(Value);
                }
            }

            BaseDevice.SendCmd(PATABase.Cmd.CacheFlush);
        }

        /// <summary>
        ///     Cleans the software and hardware caches (if any) by writing cached data to disk
        ///     if necessary before wiping the cache.
        /// </summary>
        public override void CleanCaches()
        {
            //TODO: Presumably Drive Select needs to happen first? But does the sector number 
            //      need to be set? Cannot find clarification on this anywhere.
            BaseDevice.SelectDrive(0, false);
            BaseDevice.SendCmd(PATABase.Cmd.CacheFlush);
        }
    }
}