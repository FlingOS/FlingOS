using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kernel.Hardware.PCI
{
    /// <summary>
    /// Represents a normal PCI device.
    /// </summary>
    public class PCIDeviceNormal : PCIDevice
    {
        /// <summary>
        /// The base address of the PCI device.
        /// </summary>
        public PCIBaseAddressBar[] BaseAddresses { get; private set; }

        /// <summary>
        /// The CardbusCISPointer of the device.
        /// </summary>
        public uint CardbusCISPointer { get; private set; }

        /// <summary>
        /// The device's SubsystemVendorID.
        /// </summary>
        public ushort SubsystemVendorID { get; private set; }
        /// <summary>
        /// The device's SubsystemID.
        /// </summary>
        public ushort SubsystemID { get; private set; }

        /// <summary>
        /// The device's ExpansionROMBaseAddress.
        /// </summary>
        public uint ExpansionROMBaseAddress { get; private set; }

        /// <summary>
        /// The device's CapabilitiesPointer.
        /// </summary>
        public byte CapabilitiesPointer { get; private set; }

        /// <summary>
        /// The device's MinGrant.
        /// </summary>
        public byte MinGrant { get; private set; }
        /// <summary>
        /// The device's MaxLatency.
        /// </summary>
        public byte MaxLatency { get; private set; }

        /// <summary>
        /// Initialises a new PCIDeviceNormal instance.
        /// </summary>
        /// <param name="bus">The PCI device's Bus number.</param>
        /// <param name="slot">The PCI device's Slot number.</param>
        /// <param name="function">The PCI device's Function number.</param>
        [Compiler.NoDebug]
        public PCIDeviceNormal(uint bus, uint slot, uint function)
            : base(bus, slot, function)
        {
            BaseAddresses = new PCIBaseAddressBar[6];
            BaseAddresses[0] = new PCIBaseAddressBar(ReadRegister32(0x10));
            BaseAddresses[1] = new PCIBaseAddressBar(ReadRegister32(0x14));
            BaseAddresses[2] = new PCIBaseAddressBar(ReadRegister32(0x18));
            BaseAddresses[3] = new PCIBaseAddressBar(ReadRegister32(0x1C));
            BaseAddresses[4] = new PCIBaseAddressBar(ReadRegister32(0x20));
            BaseAddresses[5] = new PCIBaseAddressBar(ReadRegister32(0x24));

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
