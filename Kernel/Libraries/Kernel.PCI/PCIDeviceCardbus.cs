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
    ///     Represents a cardbus PCI device.
    /// </summary>
    public class PCIDeviceCardbus : PCIDevice
    {
        /// <summary>
        ///     The device's CardbusBaseAddress.
        /// </summary>
        public uint CardbusBaseAddress { get; private set; }

        /// <summary>
        ///     The device's OffsetOfCapabilityList.
        /// </summary>
        public byte OffsetOfCapabilityList { get; private set; }

        /// <summary>
        ///     The device's SecondaryStatus.
        /// </summary>
        public ushort SecondaryStatus { get; private set; }

        /// <summary>
        ///     The device's PCIBusNumber.
        /// </summary>
        public byte PCIBusNumber { get; private set; }

        /// <summary>
        ///     The device's CardbusBusNumber.
        /// </summary>
        public byte CardbusBusNumber { get; private set; }

        /// <summary>
        ///     The device's SubordinateBusNumber.
        /// </summary>
        public byte SubordinateBusNumber { get; private set; }

        /// <summary>
        ///     The device's CardbusLatencyTimer.
        /// </summary>
        public byte CardbusLatencyTimer { get; private set; }

        /// <summary>
        ///     The device's MemoryBaseAddress0.
        /// </summary>
        public uint MemoryBaseAddress0 { get; private set; }

        /// <summary>
        ///     The device's MemoryLimit0.
        /// </summary>
        public uint MemoryLimit0 { get; private set; }

        /// <summary>
        ///     The device's MemoryBaseAddress1.
        /// </summary>
        public uint MemoryBaseAddress1 { get; private set; }

        /// <summary>
        ///     The device's MemoryLimit1.
        /// </summary>
        public uint MemoryLimit1 { get; private set; }

        /// <summary>
        ///     The device's IOBaseAddress0.
        /// </summary>
        public uint IOBaseAddress0 { get; private set; }

        /// <summary>
        ///     The device's IOLimit0.
        /// </summary>
        public uint IOLimit0 { get; private set; }

        /// <summary>
        ///     The device's IOBaseAddress1.
        /// </summary>
        public uint IOBaseAddress1 { get; private set; }

        /// <summary>
        ///     The device's IOLimit1.
        /// </summary>
        public uint IOLimit1 { get; private set; }

        /// <summary>
        ///     The device's BridgeControl.
        /// </summary>
        public ushort BridgeControl { get; private set; }

        /// <summary>
        ///     The device's SubsystemDeviceID.
        /// </summary>
        public ushort SubsystemDeviceID { get; private set; }

        /// <summary>
        ///     The device's SubsystemVendorID.
        /// </summary>
        public ushort SubsystemVendorID { get; private set; }

        /// <summary>
        ///     The device's PCCardBaseAddress.
        /// </summary>
        public uint PCCardBaseAddress { get; private set; }

        /// <summary>
        ///     Initialises a new PCIDeviceCardbus instance.
        /// </summary>
        /// <param name="bus">The PCI device's Bus number.</param>
        /// <param name="slot">The PCI device's Slot number.</param>
        /// <param name="function">The PCI device's Function number.</param>
        public PCIDeviceCardbus(uint bus, uint slot, uint function)
            : base(bus, slot, function, "PCI Cardbus")
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