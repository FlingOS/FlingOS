using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kernel.Hardware.PCI
{
    /// <summary>
    /// Represents a PCI base address.
    /// </summary>
    public unsafe class PCIBaseAddressBar : FOS_System.Object
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
        /// Initialises a new PCI base address.
        /// </summary>
        /// <param name="raw">The raw address value.</param>
        [Compiler.NoDebug]
        internal PCIBaseAddressBar(uint raw)
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
    }
}
