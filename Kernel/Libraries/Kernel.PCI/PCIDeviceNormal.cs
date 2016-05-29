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

namespace Kernel.PCI
{
    /// <summary>
    ///     Represents a normal PCI device.
    /// </summary>
    public class PCIDeviceNormal : PCIDevice
    {
        /// <summary>
        ///     The base address of the PCI device.
        /// </summary>
        public PCIBaseAddress[] BaseAddresses { get; }

        /// <summary>
        ///     The CardbusCISPointer of the device.
        /// </summary>
        public uint CardbusCISPointer { get; private set; }

        /// <summary>
        ///     The device's SubsystemVendorID.
        /// </summary>
        public ushort SubsystemVendorID { get; private set; }

        /// <summary>
        ///     The device's SubsystemID.
        /// </summary>
        public ushort SubsystemID { get; private set; }

        /// <summary>
        ///     The device's ExpansionROMBaseAddress.
        /// </summary>
        public uint ExpansionROMBaseAddress { get; private set; }

        /// <summary>
        ///     The device's CapabilitiesPointer.
        /// </summary>
        public byte CapabilitiesPointer { get; private set; }

        /// <summary>
        ///     The device's MinGrant.
        /// </summary>
        public byte MinGrant { get; private set; }

        /// <summary>
        ///     The device's MaxLatency.
        /// </summary>
        public byte MaxLatency { get; private set; }

        /// <summary>
        ///     Initialises a new PCIDeviceNormal instance.
        /// </summary>
        /// <param name="bus">The PCI device's Bus number.</param>
        /// <param name="slot">The PCI device's Slot number.</param>
        /// <param name="function">The PCI device's Function number.</param>
        [NoDebug]
        public PCIDeviceNormal(uint bus, uint slot, uint function)
            : base(bus, slot, function, "Normal PCI Device")
        {
            BaseAddresses = new PCIBaseAddress[6];
            BaseAddresses[0] = new PCIBaseAddress(ReadRegister32(0x10), GetSize(0));
            BaseAddresses[1] = new PCIBaseAddress(ReadRegister32(0x14), GetSize(1));
            BaseAddresses[2] = new PCIBaseAddress(ReadRegister32(0x18), GetSize(2));
            BaseAddresses[3] = new PCIBaseAddress(ReadRegister32(0x1C), GetSize(3));
            BaseAddresses[4] = new PCIBaseAddress(ReadRegister32(0x20), GetSize(4));
            BaseAddresses[5] = new PCIBaseAddress(ReadRegister32(0x24), GetSize(5));

            CardbusCISPointer = ReadRegister32(0x28);

            SubsystemVendorID = ReadRegister16(0x2C);
            SubsystemID = ReadRegister16(0x2E);

            ExpansionROMBaseAddress = ReadRegister32(0x30);

            CapabilitiesPointer = ReadRegister8(0x34);

            MinGrant = ReadRegister8(0x3E);
            MaxLatency = ReadRegister8(0x3F);
        }
    }
}