#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.USB
{
    public enum USBTransferType : byte
    {
        USB_BULK, USB_CONTROL, USB_INTERRUPT, USB_ISOCHRONOUS
    }

    public unsafe class USBTransfer : FOS_System.Object
    {
        public void* data;
        public USBTransferType type;
        public byte endpoint;
        public ushort    packetSize;
        public USB.Devices.USBDeviceInfo device;
        public List transactions;
        public bool success;
    }
}
