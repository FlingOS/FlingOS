using Drivers.Compiler.Attributes;

namespace Kernel.PCI
{
    public class PCIVirtualNormalDevice : PCIVirtualDevice
    {
        /// <summary>
        ///     The base address of the PCI device.
        /// </summary>
        public PCIBaseAddress[] BaseAddresses { get; private set; }

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
        public PCIVirtualNormalDevice(uint bus, uint slot, uint function)
            : base(bus, slot, function)
        {
            Init();
        }

        /// <summary>
        ///     Initialises a new PCIDeviceNormal instance.
        /// </summary>
        /// <param name="bus">The PCI device's Bus number.</param>
        /// <param name="slot">The PCI device's Slot number.</param>
        /// <param name="function">The PCI device's Function number.</param>
        public PCIVirtualNormalDevice(uint bus, uint slot, uint function,
            PCIDevice.PCIHeaderType headertype, byte classcode, byte subclass)
            : base(bus, slot, function, headertype, classcode, subclass)
        {
            Init();
        }

        private void Init()
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
