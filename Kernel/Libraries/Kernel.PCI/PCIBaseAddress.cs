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

using Drivers.Compiler.Attributes;
using Kernel.Framework;

namespace Kernel.PCI
{
    /// <summary>
    ///     Represents a PCI base address.
    /// </summary>
    public unsafe class PCIBaseAddress : Object
    {
        /// <summary>
        ///     The underlying base address pointer.
        /// </summary>
        private readonly byte* baseAddress;

        /// <summary>
        ///     Whether the base address is an IO address.
        /// </summary>
        private readonly bool isIO;

        /// <summary>
        ///     The PCI device's required memory size.
        /// </summary>
        private readonly uint size;

        /// <summary>
        ///     The base address type.
        /// </summary>
        private readonly byte type;

        /// <summary>
        ///     Whether the data is prefetchable or not.
        /// </summary>
        private ushort prefetchable;

        /// <summary>
        ///     Initialises a new PCI base address.
        /// </summary>
        /// <param name="raw">The raw address value.</param>
        /// <param name="aSize">The size of the PCI registers pointed to by the base address.</param>
        [NoDebug]
        internal PCIBaseAddress(uint raw, uint aSize)
        {
            //Determine whether this is an IO port base address
            //  or a memory-mapped base address
            isIO = (raw & 0x01) == 1;

            if (isIO)
            {
                //Bit 0 is IO indicator
                //Bit 1 is reserved
                //4-byte aligned base port address
                baseAddress = (byte*)(raw & 0xFFFFFFFC);
            }
            else
            {
                //Type indicator is bits 1 and 2
                /* *********************** Description taken from OSDev.org ***************************
                 * The Type field of the Memory Space BAR Layout specifies the size of the base       *
                 * register and where in memory it can be mapped. If it has a value of 0x00 then the  *
                 * base register is 32-bits wide and can be mapped anywhere in the 32-bit Memory      *
                 * Space. A value of 0x02 means the base register is 64-bits wide and can be mapped   *
                 * anywhere in the 64-bit Memory Space (A 64-bit base address register consumes 2 of  *
                 * the base address registers available). A value of 0x01 is reserved as of revision  *
                 * 3.0 of the PCI Local Bus Specification. In earlier versions it was used to support *
                 * memory space below 1MB (16-bit wide base register that can be mapped anywhere in   *
                 * the 16-bit Memory Space).                                                          *
                 *                                                                                    *
                 * Source: http://wiki.osdev.org/PCI#Base_Address_Registers, para 2, 1st Sep 2014     */
                type = (byte)((raw >> 1) & 0x03);
                //Bit 3 - Determines whether you can prefetch data from memory or not (in C this would
                //  determine whether something is volatile or not.)
                prefetchable = (ushort)((raw >> 3) & 0x01);
                switch (type)
                {
                    case 0x00:
                        baseAddress = (byte*)(raw & 0xFFFFFFF0);
                        break;
                    case 0x01:
                        baseAddress = (byte*)(raw & 0xFFFFFFF0);
                        break;
                    //Type 0x2 - 64-bit address not supported
                }
            }

            size = aSize;
        }

        /// <summary>
        ///     The base address byte pointer.
        /// </summary>
        /// <returns>Returns the base address byte pointer.</returns>
        public byte* BaseAddress()
        {
            return baseAddress;
        }

        /// <summary>
        ///     Whether the base address is IO or not.
        /// </summary>
        /// <returns>Whether the base address is IO or not.</returns>
        public bool IsIO()
        {
            return isIO;
        }

        /// <summary>
        ///     The PCI device's required memory size.
        /// </summary>
        /// <returns>The size.</returns>
        public uint Size()
        {
            return size;
        }
    }
}