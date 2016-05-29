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

using Kernel.Framework;
using Kernel.Framework.Processes.Requests.Devices;

namespace Kernel.Devices
{
    /// <summary>
    ///     Represents a logical block based device.
    /// </summary>
    public abstract class BlockDevice : Device
    {
        /// <summary>
        ///     The size of the logical blocks.
        /// </summary>
        protected ulong blockSize = 0;

        /// <summary>
        ///     The number of logical blocks in the device.
        /// </summary>
        public virtual ulong Blocks { get; protected set; }

        /// <summary>
        ///     The size of the logical blocks.
        /// </summary>
        public virtual ulong BlockSize => blockSize;

        /// <summary>
        ///     Initialises a new block device with the specified information.
        /// </summary>
        /// <param name="Group">The device group of the device.</param>
        /// <param name="Class">The device class of the device.</param>
        /// <param name="SubClass">The device subclass of the device.</param>
        /// <param name="Name">The human-readable name of the device.</param>
        /// <param name="SomeInfo">A device-specific array of information to store with the device.</param>
        /// <param name="IsClaimed">Whether the device has already been claimed by a driver or not.</param>
        public BlockDevice(DeviceGroup Group, DeviceClass Class, DeviceSubClass SubClass, String Name,
            uint[] SomeInfo, bool IsClaimed)
            : base(Group, Class, SubClass, Name, SomeInfo, IsClaimed)
        {
        }

        /// <summary>
        ///     Reads contiguous logical blocks from the device.
        /// </summary>
        /// <param name="BlockNo">The logical block number to read.</param>
        /// <param name="BlockCount">The number of blocks to read.</param>
        /// <param name="Data">The byte array to store the data in.</param>
        public abstract void ReadBlock(ulong BlockNo, uint BlockCount, byte[] Data);

        /// <summary>
        ///     Writes contiguous logical blocks to the device.
        /// </summary>
        /// <param name="BlockNo">The number of the first block to write.</param>
        /// <param name="BlockCount">The number of blocks to write.</param>
        /// <param name="Data">The data to write. Pass null to efficiently write 0s to the device.</param>
        /// <remarks>
        ///     If data is null, all data to be written should be assumed to be 0.
        /// </remarks>
        public abstract void WriteBlock(ulong BlockNo, uint BlockCount, byte[] Data);

        /// <summary>
        ///     Creates a new byte array sized to fit the specified number of blocks.
        /// </summary>
        /// <param name="aBlockCount">The number of blocks to size for.</param>
        /// <returns>The new byte array.</returns>
        public byte[] NewBlockArray(uint aBlockCount)
        {
            //TODO: Support Conv_Ovf_I_Un IL op then remove the cast below
            return new byte[aBlockCount*(uint)BlockSize];
        }
    }
}