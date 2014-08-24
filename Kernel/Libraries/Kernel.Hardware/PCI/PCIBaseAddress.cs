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
            isIO = (raw & 0x01) == 1;

            if (isIO)
            {
                baseAddress = (byte*)(raw & 0xFFFFFFFC);
            }
            else
            {
                type = (byte)((raw >> 1) & 0x03);
                prefetchable = (ushort)((raw >> 3) & 0x01);
                switch (type)
                {
                    case 0x00:
                        baseAddress = (byte*)(raw & 0xFFFFFFF0);
                        break;
                    case 0x01:
                        baseAddress = (byte*)(raw & 0xFFFFFFF0);
                        break;
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
