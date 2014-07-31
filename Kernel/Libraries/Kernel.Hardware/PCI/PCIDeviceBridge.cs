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
    /// Represents a bridge PCI device.
    /// </summary>
    public class PCIDeviceBridge : PCIDevice
    {
        /// <summary>
        /// The device's BaseAddresses.
        /// </summary>
        public PCIBaseAddress[] BaseAddresses { get; private set; }

        /// <summary>
        /// The device's PrimaryBusNumber.
        /// </summary>
        public byte PrimaryBusNumber { get; private set; }
        /// <summary>
        /// The device's SecondaryBusNumber.
        /// </summary>
        public byte SecondaryBusNumber { get; private set; }
        /// <summary>
        /// The device's SubordinateBusNumber.
        /// </summary>
        public byte SubordinateBusNumber { get; private set; }
        /// <summary>
        /// The device's SecondaryLatencyTimer.
        /// </summary>
        public byte SecondaryLatencyTimer { get; private set; }

        /// <summary>
        /// The device's IOBase.
        /// </summary>
        public byte IOBase { get; private set; }
        /// <summary>
        /// The device's IOLimit.
        /// </summary>
        public byte IOLimit { get; private set; }
        /// <summary>
        /// The device's SecondaryStatus.
        /// </summary>
        public ushort SecondaryStatus { get; private set; }

        /// <summary>
        /// The device's MemoryBase.
        /// </summary>
        public ushort MemoryBase { get; private set; }
        /// <summary>
        /// The device's MemoryLimit.
        /// </summary>
        public ushort MemoryLimit { get; private set; }

        /// <summary>
        /// The device's PrefatchableMemoryBase.
        /// </summary>
        public ushort PrefatchableMemoryBase { get; private set; }
        /// <summary>
        /// The device's PrefatchableMemoryLimit.
        /// </summary>
        public ushort PrefatchableMemoryLimit { get; private set; }

        /// <summary>
        /// The device's PrefatchableBaseUpper32.
        /// </summary>
        public uint PrefatchableBaseUpper32 { get; private set; }

        /// <summary>
        /// The device's PrefatchableLimitUpper32.
        /// </summary>
        public uint PrefatchableLimitUpper32 { get; private set; }

        /// <summary>
        /// The device's IOBaseUpper16.
        /// </summary>
        public ushort IOBaseUpper16 { get; private set; }
        /// <summary>
        /// The device's IOLimitUpper16.
        /// </summary>
        public ushort IOLimitUpper16 { get; private set; }

        /// <summary>
        /// The device's CapabilityPointer.
        /// </summary>
        public byte CapabilityPointer { get; private set; }

        /// <summary>
        /// The device's ExpansionROMBaseAddress.
        /// </summary>
        public uint ExpansionROMBaseAddress { get; private set; }

        /// <summary>
        /// The device's BridgeControl.
        /// </summary>
        public ushort BridgeControl { get; private set; }

        /// <summary>
        /// Initialises a new PCIDeviceBridge instance.
        /// </summary>
        /// <param name="bus">The PCI device's Bus number.</param>
        /// <param name="slot">The PCI device's Slot number.</param>
        /// <param name="function">The PCI device's Function number.</param>
        public PCIDeviceBridge(uint bus, uint slot, uint function)
            : base(bus, slot, function)
        {
            BaseAddresses = new PCIBaseAddress[2];
            BaseAddresses[0] = new PCIBaseAddress(ReadRegister32(0x10), GetSize(0));
            BaseAddresses[1] = new PCIBaseAddress(ReadRegister32(0x14), GetSize(1));

            PrimaryBusNumber = ReadRegister8(0x18);
            SecondaryBusNumber = ReadRegister8(0x19);
            SubordinateBusNumber = ReadRegister8(0x1A);
            SecondaryLatencyTimer = ReadRegister8(0x1B);

            IOBase = ReadRegister8(0x1C);
            IOLimit = ReadRegister8(0x1D);
            SecondaryStatus = ReadRegister16(0x1E);

            MemoryBase = ReadRegister16(0x20);
            MemoryLimit = ReadRegister16(0x22);

            PrefatchableMemoryBase = ReadRegister16(0x24);
            PrefatchableMemoryLimit = ReadRegister16(0x26);

            PrefatchableBaseUpper32 = ReadRegister32(0x28);
            PrefatchableLimitUpper32 = ReadRegister32(0x2C);

            IOBaseUpper16 = ReadRegister16(0x30);
            IOLimitUpper16 = ReadRegister16(0x32);

            CapabilityPointer = ReadRegister8(0x34);

            ExpansionROMBaseAddress = ReadRegister32(0x38);

            BridgeControl = ReadRegister16(0x3E);
        }
    }
}
