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

namespace Kernel.Hardware.PCI
{
    /// <summary>
    /// Represents a PCI base address.
    /// </summary>
    public unsafe class PCIBaseAddress : FOS_System.Object
    {
        /// <summary>
        /// The underlying base address pointer.
        /// </summary>
        private byte* baseAddress;
        /// <summary>
        /// Whether the data is prefetchable or not.
        /// </summary>
        private ushort prefetchable = 0;
        /// <summary>
        /// The base address type.
        /// </summary>
        private byte type = 0;
        /// <summary>
        /// Whether the base address is an IO address.
        /// </summary>
        private bool isIO = false;

        /// <summary>
        /// The PCI device's required memory size.
        /// </summary>
        private uint size = 0;

        /// <summary>
        /// Initialises a new PCI base address.
        /// </summary>
        /// <param name="raw">The raw address value.</param>
        /// <param name="aSize">The size of the PCI registers pointed to by the base address.</param>
        [Compiler.NoDebug]
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
        /// The base address byte pointer.
        /// </summary>
        /// <returns>Returns the base address byte pointer.</returns>
        public byte* BaseAddress()
        {
            return baseAddress;
        }

        /// <summary>
        /// Whether the base address is IO or not.
        /// </summary>
        /// <returns>Whether the base address is IO or not.</returns>
        public bool IsIO()
        {
            return isIO;
        }

        /// <summary>
        /// The PCI device's required memory size.
        /// </summary>
        /// <returns>The size.</returns>
        public uint Size()
        {
            return size;
        }
    }
}
