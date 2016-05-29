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

namespace Kernel.PCI
{
    /// <summary>
    ///     Represents a bridge PCI device.
    /// </summary>
    public class PCIDeviceBridge : PCIDevice
    {
        /// <summary>
        ///     The device's BaseAddresses.
        /// </summary>
        public PCIBaseAddress[] BaseAddresses { get; }

        /// <summary>
        ///     The device's PrimaryBusNumber.
        /// </summary>
        public byte PrimaryBusNumber { get; private set; }

        /// <summary>
        ///     The device's SecondaryBusNumber.
        /// </summary>
        public byte SecondaryBusNumber { get; private set; }

        /// <summary>
        ///     The device's SubordinateBusNumber.
        /// </summary>
        public byte SubordinateBusNumber { get; private set; }

        /// <summary>
        ///     The device's SecondaryLatencyTimer.
        /// </summary>
        public byte SecondaryLatencyTimer { get; private set; }

        /// <summary>
        ///     The device's IOBase.
        /// </summary>
        public byte IOBase { get; private set; }

        /// <summary>
        ///     The device's IOLimit.
        /// </summary>
        public byte IOLimit { get; private set; }

        /// <summary>
        ///     The device's SecondaryStatus.
        /// </summary>
        public ushort SecondaryStatus { get; private set; }

        /// <summary>
        ///     The device's MemoryBase.
        /// </summary>
        public ushort MemoryBase { get; private set; }

        /// <summary>
        ///     The device's MemoryLimit.
        /// </summary>
        public ushort MemoryLimit { get; private set; }

        /// <summary>
        ///     The device's PrefatchableMemoryBase.
        /// </summary>
        public ushort PrefatchableMemoryBase { get; private set; }

        /// <summary>
        ///     The device's PrefatchableMemoryLimit.
        /// </summary>
        public ushort PrefatchableMemoryLimit { get; private set; }

        /// <summary>
        ///     The device's PrefatchableBaseUpper32.
        /// </summary>
        public uint PrefatchableBaseUpper32 { get; private set; }

        /// <summary>
        ///     The device's PrefatchableLimitUpper32.
        /// </summary>
        public uint PrefatchableLimitUpper32 { get; private set; }

        /// <summary>
        ///     The device's IOBaseUpper16.
        /// </summary>
        public ushort IOBaseUpper16 { get; private set; }

        /// <summary>
        ///     The device's IOLimitUpper16.
        /// </summary>
        public ushort IOLimitUpper16 { get; private set; }

        /// <summary>
        ///     The device's CapabilityPointer.
        /// </summary>
        public byte CapabilityPointer { get; private set; }

        /// <summary>
        ///     The device's ExpansionROMBaseAddress.
        /// </summary>
        public uint ExpansionROMBaseAddress { get; private set; }

        /// <summary>
        ///     The device's BridgeControl.
        /// </summary>
        public ushort BridgeControl { get; private set; }

        /// <summary>
        ///     Initialises a new PCIDeviceBridge instance.
        /// </summary>
        /// <param name="bus">The PCI device's Bus number.</param>
        /// <param name="slot">The PCI device's Slot number.</param>
        /// <param name="function">The PCI device's Function number.</param>
        public PCIDeviceBridge(uint bus, uint slot, uint function)
            : base(bus, slot, function, "PCI Device Bridge")
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