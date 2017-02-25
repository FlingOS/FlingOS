using Drivers.Compiler.Attributes;
using Kernel.Framework;

namespace Kernel.PCI
{
    public class PCIVirtualDevice : Object
    {
        /// <summary>
        ///     The device's bus number.
        /// </summary>
        public uint bus;

        /// <summary>
        ///     The device's function number.
        /// </summary>
        public uint function;

        /// <summary>
        ///     The device's slot number.
        /// </summary>
        public uint slot;

        /// <summary>
        ///     The device's VendorID.
        /// </summary>
        public ushort VendorID { get; private set; }

        /// <summary>
        ///     The device's DeviceID.
        /// </summary>
        public ushort DeviceID { get; private set; }

        /// <summary>
        ///     Reads / writes the command register.
        /// </summary>
        public PCIDevice.PCICommand Command
        {
            get { return (PCIDevice.PCICommand)ReadRegister16(0x04); }
            set { WriteRegister16(0x04, (ushort)value); }
        }

        /// <summary>
        ///     Reads / writes the status register.
        /// </summary>
        public PCIDevice.PCIStatus Status
        {
            get { return (PCIDevice.PCIStatus)ReadRegister16(0x06); }
            set { WriteRegister16(0x06, (ushort)value); }
        }

        /// <summary>
        ///     The device's RevisionID.
        /// </summary>
        public byte RevisionID { get; private set; }

        /// <summary>
        ///     The device's ProgIF.
        /// </summary>
        public byte ProgIF { get; private set; }

        /// <summary>
        ///     The device's Subclass.
        /// </summary>
        public byte Subclass { get; }

        /// <summary>
        ///     The device's ClassCode.
        /// </summary>
        public byte ClassCode { get; }

        /// <summary>
        ///     The device's CacheLineSize.
        /// </summary>
        public byte CacheLineSize { get; private set; }

        /// <summary>
        ///     The device's LatencyTimer.
        /// </summary>
        public byte LatencyTimer { get; private set; }

        /// <summary>
        ///     The device's HeaderType.
        /// </summary>
        public PCIDevice.PCIHeaderType HeaderType { get; }

        /// <summary>
        ///     The device's BIST.
        /// </summary>
        public PCIDevice.PCIBISTs BIST { get; private set; }

        /// <summary>
        ///     The device's InterruptLine.
        /// </summary>
        public byte InterruptLine { get; private set; }

        /// <summary>
        ///     The device's InterruptPIN.
        /// </summary>
        public PCIDevice.PCIInterruptPIN InterruptPIN { get; private set; }

        /// <summary>
        ///     The device's DeviceExists.
        /// </summary>
        public bool DeviceExists { get; private set; }

        /// <summary>
        ///     Initialises a new, generic, virtual PCI device.
        /// </summary>
        /// <param name="bus">The PCI bus number.</param>
        /// <param name="slot">The PCI slot number.</param>
        /// <param name="function">The PCI function number.</param>
        [NoDebug]
        public PCIVirtualDevice(uint bus, uint slot, uint function)
        {
            PCIAccessor.Init();

            this.bus = bus;
            this.slot = slot;
            this.function = function;

            Subclass = ReadRegister8(0x0A);
            ClassCode = ReadRegister8(0x0B);
            HeaderType = (PCIDevice.PCIHeaderType)ReadRegister8(0x0E);
            
            //BasicConsole.WriteLine((String)"PCI Virtual Device: Header type: " + (byte)HeaderType);
            //BasicConsole.WriteLine((String)"PCI Virtual Device:  Class code: " + ClassCode);
            //BasicConsole.WriteLine((String)"PCI Virtual Device:   Sub class: " + Subclass);
        }

        /// <summary>
        ///     Initialises a new, generic, virtual PCI device.
        /// </summary>
        /// <param name="bus">The PCI bus number.</param>
        /// <param name="slot">The PCI slot number.</param>
        /// <param name="function">The PCI function number.</param>
        [NoDebug]
        public PCIVirtualDevice(uint bus, uint slot, uint function,
            PCIDevice.PCIHeaderType headertype, byte classcode, byte subclass)
        {
            PCIAccessor.Init();

            this.bus = bus;
            this.slot = slot;
            this.function = function;

            Subclass = subclass;
            ClassCode = classcode;
            HeaderType = headertype;

            //BasicConsole.WriteLine((String)"PCI Virtual Device: Header type: " + (byte)HeaderType);
            //BasicConsole.WriteLine((String)"PCI Virtual Device:  Class code: " + ClassCode);
            //BasicConsole.WriteLine((String)"PCI Virtual Device:   Sub class: " + Subclass);
        }

        public void LoadRemainingRegisters()
        {
            VendorID = ReadRegister16(0x00);
            DeviceID = ReadRegister16(0x02);

            RevisionID = ReadRegister8(0x08);
            ProgIF = ReadRegister8(0x09);

            CacheLineSize = ReadRegister8(0x0C);
            LatencyTimer = ReadRegister8(0x0D);

            BIST = (PCIDevice.PCIBISTs)ReadRegister8(0x0F);

            InterruptLine = ReadRegister8(0x3C);
            InterruptPIN = (PCIDevice.PCIInterruptPIN)ReadRegister8(0x3D);

            DeviceExists = (uint)VendorID != 0xFFFF && (uint)DeviceID != 0xFFFF;
        }

        /// <summary>
        ///     Calculates the base address for a PCI device.
        /// </summary>
        /// <param name="aBus">PCI bus number.</param>
        /// <param name="aSlot">PCI slot number.</param>
        /// <param name="aFunction">PCI function number.</param>
        /// <returns>The base address.</returns>
        [NoDebug]
        protected static uint GetAddressBase(uint aBus, uint aSlot, uint aFunction)
        {
            // 31 	        30 - 24    23 - 16      15 - 11 	    10 - 8 	          7 - 2 	        1 - 0
            // Enable Bit 	Reserved   Bus Number 	Device Number 	Function Number   Register Number 	00 
            return 0x80000000
                   // Bits 23-16
                   | (aBus << 16)
                   // Bits 15-11
                   | ((aSlot & 0x1F) << 11)
                   // Bits 10-8
                   | ((aFunction & 0x07) << 8);
        }

        /// <summary>
        ///     Gets the size associated with the specified base address register.
        /// </summary>
        /// <param name="bar">The number of the base address register to test.</param>
        /// <returns>The size.</returns>
        protected uint GetSize(byte bar)
        {
            //Calculate register number for specified BAR number
            byte regNum = (byte)(0x10 + bar * 4);
            //Read BAR address into a temp store so we can restore it later
            uint baseAddr = ReadRegister32(regNum);
            //As per spec:

            //Write all 1s to base address register
            WriteRegister32(regNum, 0xFFFFFFFF);
            //Read back the value
            uint size = ReadRegister32(regNum);
            //Invert the bits of the size, OR with 0xF and then add 1
            size = (~size | 0x0F) + 1;

            //Restore the base address
            WriteRegister32(regNum, baseAddr);

            //Return the size
            return size;
        }


        /// <summary>
        ///     Reads a byte from the specified register.
        /// </summary>
        /// <param name="aRegister">The register to read.</param>
        /// <returns>The byte that has been read.</returns>
        [NoDebug]
        public byte ReadRegister8(byte aRegister)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | (uint)(aRegister & 0xFC);
            return (byte)(PCIAccessor.AccessPorts(xAddr, true, 4, 0) >> (aRegister % 4 * 8));
        }

        /// <summary>
        ///     Writes a byte to the specified register.
        /// </summary>
        /// <param name="aRegister">The register to write.</param>
        /// <param name="value">The value to write.</param>
        [NoDebug]
        public void WriteRegister8(byte aRegister, byte value)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | (uint)(aRegister & 0xFC);
            PCIAccessor.AccessPorts(xAddr, false, 4, ReadRegister32(aRegister) & ~(0xFFu << (aRegister % 4 * 8)) | ((uint)value << (aRegister % 4 * 8)));
        }

        /// <summary>
        ///     Reads a UInt16 from the specified register.
        /// </summary>
        /// <param name="aRegister">The register to read.</param>
        /// <returns>The UInt16 that has been read.</returns>
        [NoDebug]
        public ushort ReadRegister16(byte aRegister)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | (uint)(aRegister & 0xFC);
            return (ushort)(PCIAccessor.AccessPorts(xAddr, true, 4, 0) >> (aRegister % 4 * 8));
        }

        /// <summary>
        ///     Writes a UInt16 to the specified register.
        /// </summary>
        /// <param name="aRegister">The register to write.</param>
        /// <param name="value">The value to write.</param>
        [NoDebug]
        public void WriteRegister16(byte aRegister, ushort value)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | (uint)(aRegister & 0xFC);
            PCIAccessor.AccessPorts(xAddr, false, 4, ReadRegister32(aRegister) & ~(0xFFFFu << (aRegister % 4 * 8)) | ((uint)value << (aRegister % 4 * 8)));
        }

        /// <summary>
        ///     Reads a UInt32 from the specified register.
        /// </summary>
        /// <param name="aRegister">The register to read.</param>
        /// <returns>The UInt32 that has been read.</returns>
        [NoDebug]
        public uint ReadRegister32(byte aRegister)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | (uint)(aRegister & 0xFC);
            return (PCIAccessor.AccessPorts(xAddr, true, 4, 0) >> (aRegister % 4 * 8));
        }

        /// <summary>
        ///     Writes a UInt32 to the specified register.
        /// </summary>
        /// <param name="aRegister">The register to write.</param>
        /// <param name="value">The value to write.</param>
        [NoDebug]
        public void WriteRegister32(byte aRegister, uint value)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | (uint)(aRegister & 0xFC);
            PCIAccessor.AccessPorts(xAddr, false, 4, value);
        }
    }
}
