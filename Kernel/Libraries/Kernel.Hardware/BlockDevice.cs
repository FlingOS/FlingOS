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

namespace Kernel.Hardware
{
    /// <summary>
    /// Represents a logical block based device.
    /// </summary>
    public abstract class BlockDevice : Device
    {
        /// <summary>
        /// The number of logical blocks in the device.
        /// </summary>
        protected UInt64 blockCount = 0;
        /// <summary>
        /// The number of logical blocks in the device.
        /// </summary>
        public UInt64 BlockCount
        {
            get { return blockCount; }
        }

        /// <summary>
        /// The size of the logical blocks.
        /// </summary>
        protected UInt64 blockSize = 0;
        /// <summary>
        /// The size of the logical blocks.
        /// </summary>
        public UInt64 BlockSize
        {
            get { return blockSize; }
        }

        /// <summary>
        /// Reads contiguous logical blocks from the device.
        /// </summary>
        /// <param name="aBlockNo">The logical block number to read.</param>
        /// <param name="aBlockCount">The number of blocks to read.</param>
        /// <param name="aData">The byte array to store the data in.</param>
        public abstract void ReadBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData);
        /// <summary>
        /// Writes contiguous logical blocks to the device.
        /// </summary>
        /// <param name="aBlockNo">The number of the first block to write.</param>
        /// <param name="aBlockCount">The number of blocks to write.</param>
        /// <param name="aData">The data to write. Pass null to efficiently write 0s to the device.</param>
        /// <remarks>
        /// If data is null, all data to be written should be assumed to be 0.
        /// </remarks>
        public abstract void WriteBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData);

        /// <summary>
        /// Creates a new byte array sized to fit the specified number of blocks.
        /// </summary>
        /// <param name="aBlockCount">The number of blocks to size for.</param>
        /// <returns>The new byte array.</returns>
        public byte[] NewBlockArray(UInt32 aBlockCount)
        {
            //TODO - Err..this cast here is really really bad practice but it's just because 
            //  we can't do 64 bit x 64 bit multiplication nor support the conv.ovf.i.un op
            return new byte[aBlockCount * (UInt32)blockSize];
        }
    }
}
