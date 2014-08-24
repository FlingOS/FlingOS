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
    /// Represents a cardbus PCI device.
    /// </summary>
    public class PCIDeviceCardbus : PCIDevice
    {
        /// <summary>
        /// The device's CardbusBaseAddress.
        /// </summary>
        public uint CardbusBaseAddress { get; private set; }

        /// <summary>
        /// The device's OffsetOfCapabilityList.
        /// </summary>
        public byte OffsetOfCapabilityList { get; private set; }
        /// <summary>
        /// The device's SecondaryStatus.
        /// </summary>
        public ushort SecondaryStatus { get; private set; }

        /// <summary>
        /// The device's PCIBusNumber.
        /// </summary>
        public byte PCIBusNumber { get; private set; }
        /// <summary>
        /// The device's CardbusBusNumber.
        /// </summary>
        public byte CardbusBusNumber { get; private set; }
        /// <summary>
        /// The device's SubordinateBusNumber.
        /// </summary>
        public byte SubordinateBusNumber { get; private set; }
        /// <summary>
        /// The device's CardbusLatencyTimer.
        /// </summary>
        public byte CardbusLatencyTimer { get; private set; }

        /// <summary>
        /// The device's MemoryBaseAddress0.
        /// </summary>
        public uint MemoryBaseAddress0 { get; private set; }
        /// <summary>
        /// The device's MemoryLimit0.
        /// </summary>
        public uint MemoryLimit0 { get; private set; }
        /// <summary>
        /// The device's MemoryBaseAddress1.
        /// </summary>
        public uint MemoryBaseAddress1 { get; private set; }
        /// <summary>
        /// The device's MemoryLimit1.
        /// </summary>
        public uint MemoryLimit1 { get; private set; }

        /// <summary>
        /// The device's IOBaseAddress0.
        /// </summary>
        public uint IOBaseAddress0 { get; private set; }
        /// <summary>
        /// The device's IOLimit0.
        /// </summary>
        public uint IOLimit0 { get; private set; }
        /// <summary>
        /// The device's IOBaseAddress1.
        /// </summary>
        public uint IOBaseAddress1 { get; private set; }
        /// <summary>
        /// The device's IOLimit1.
        /// </summary>
        public uint IOLimit1 { get; private set; }

        /// <summary>
        /// The device's BridgeControl.
        /// </summary>
        public ushort BridgeControl { get; private set; }

        /// <summary>
        /// The device's SubsystemDeviceID.
        /// </summary>
        public ushort SubsystemDeviceID { get; private set; }
        /// <summary>
        /// The device's SubsystemVendorID.
        /// </summary>
        public ushort SubsystemVendorID { get; private set; }

        /// <summary>
        /// The device's PCCardBaseAddress.
        /// </summary>
        public uint PCCardBaseAddress { get; private set; }

        /// <summary>
        /// Initialises a new PCIDeviceCardbus instance.
        /// </summary>
        /// <param name="bus">The PCI device's Bus number.</param>
        /// <param name="slot">The PCI device's Slot number.</param>
        /// <param name="function">The PCI device's Function number.</param>
        public PCIDeviceCardbus(uint bus, uint slot, uint function)
            : base(bus, slot, function)
        {
            CardbusBaseAddress = ReadRegister32(0x10);

            OffsetOfCapabilityList = ReadRegister8(0x14);
            SecondaryStatus = ReadRegister16(0x16);

            PCIBusNumber = ReadRegister8(0x18);
            CardbusBusNumber = ReadRegister8(0x19);
            SubordinateBusNumber = ReadRegister8(0x1A);
            CardbusLatencyTimer = ReadRegister8(0x1B);

            MemoryBaseAddress0 = ReadRegister32(0x1C);
            MemoryLimit0 = ReadRegister32(0x20);
            MemoryBaseAddress1 = ReadRegister32(0x24);
            MemoryLimit1 = ReadRegister32(0x28);

            IOBaseAddress0 = ReadRegister32(0x2C);
            IOLimit0 = ReadRegister32(0x30);
            IOBaseAddress1 = ReadRegister32(0x34);
            IOLimit1 = ReadRegister32(0x38);

            BridgeControl = ReadRegister16(0x3C);

            SubsystemDeviceID = ReadRegister16(0x40);
            SubsystemVendorID = ReadRegister16(0x42);

            PCCardBaseAddress = ReadRegister32(0x44);
        }
    }
}
