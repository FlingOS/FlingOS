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
using Kernel.Framework.Collections;
using Kernel.USB.Devices;

namespace Kernel.USB
{
    /// <summary>
    ///     The USB transfer types.
    /// </summary>
    public enum USBTransferType : byte
    {
        /// <summary>
        ///     Indicates a bulk transfer.
        /// </summary>
        Bulk,

        /// <summary>
        ///     Indicates a control transfer.
        /// </summary>
        Control,

        /// <summary>
        ///     Indicates an interrupt transfer.
        /// </summary>
        Interrupt,

        /// <summary>
        ///     Indicates an isochronous transfer.
        /// </summary>
        Isochronous
    }

    /// <summary>
    ///     Represents a transfer from the high-level USB perspective.
    /// </summary>
    public unsafe class USBTransfer : Object
    {
        /// <summary>
        ///     The device info of the device which owns the target endpoint.
        /// </summary>
        public USBDeviceInfo device;

        /// <summary>
        ///     The endpoint to send the transfer to.
        /// </summary>
        public byte endpoint;

        /// <summary>
        ///     The preferred size of the packets to use when sending/receiving transactions within the transfer.
        /// </summary>
        public ushort packetSize;

        /// <summary>
        ///     Whether the transfer completed successfully (in-full) or not.
        /// </summary>
        public bool success;

        /// <summary>
        ///     The list of <see cref="USBTransaction"/>s to send.
        /// </summary>
        /// <seealso cref="Kernel.USB.USBTransaction" />
        public List transactions;

        /// <summary>
        ///     The transfer type.
        /// </summary>
        public USBTransferType type;

        /// <summary>
        ///     A pointer to the underlying data which a specific host controller can actually use to execute the transfer.
        /// </summary>
        public void* underlyingTransferData;
    }
}