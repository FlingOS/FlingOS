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
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.USB
{
    /// <summary>
    /// The USB transfer types.
    /// </summary>
    public enum USBTransferType : byte
    {
        /// <summary>
        /// Indicates a bulk transfer.
        /// </summary>
        Bulk,
        /// <summary>
        /// Indicates a control transfer.
        /// </summary>
        Control,
        /// <summary>
        /// Indicates an interrupt transfer.
        /// </summary>
        Interrupt,
        /// <summary>
        /// Indicates an isochronous transfer.
        /// </summary>
        Isochronous
    }
    /// <summary>
    /// Represents a transfer from the high-level USB perspective.
    /// </summary>
    public unsafe class USBTransfer : FOS_System.Object
    {
        /// <summary>
        /// A pointer to the underlying data which a specific host controller can actually use to execute the transfer.
        /// </summary>
        public void* underlyingTransferData;
        /// <summary>
        /// The transfer type.
        /// </summary>
        public USBTransferType type;
        /// <summary>
        /// The endpoint to send the transfer to.
        /// </summary>
        public byte endpoint;
        /// <summary>
        /// The preferred size of the packets to use when sending/receiving transactions within the transfer.
        /// </summary>
        public ushort packetSize;
        /// <summary>
        /// The device info of the device which owns the target endpoint.
        /// </summary>
        public USB.Devices.USBDeviceInfo device;
        /// <summary>
        /// The list of "USBTransaction"s to send.
        /// </summary>
        /// <seealso cref="Kernel.Hardware.USB.USBTransaction"/>
        public List transactions;
        /// <summary>
        /// Whether the transfer completed succesffuly (in-full) or not.
        /// </summary>
        public bool success;
    }
}
