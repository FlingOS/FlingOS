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

using System;
using Drivers.Compiler.Attributes;
using Kernel.Devices;
using Kernel.Framework.Processes.Requests.Devices;
using PCI_IO = Kernel.PCI.PCIManager;
using String = Kernel.Framework.String;

namespace Kernel.PCI
{
    /// <summary>
    ///     Represents any PCI device.
    /// </summary>
    public class PCIDevice : Device
    {
        /// <summary>
        ///     Built-in Self Test status byte masks.
        /// </summary>
        [Flags]
        public enum PCIBISTs : byte
        {
            /// <summary>
            ///     Return result mask.
            /// </summary>
            CocdMask = 0x0f,

            /// <summary>
            ///     Start mask.
            /// </summary>
            Start = 0x40,

            /// <summary>
            ///     BIST Capable mask.
            /// </summary>
            Capable = 0x80
        }

        /// <summary>
        ///     PCI command masks.
        /// </summary>
        [Flags]
        public enum PCICommand : ushort
        {
            /// <summary>
            ///     Enable response in I/O space command.
            /// </summary>
            IO = 0x1,

            /// <summary>
            ///     Enable response in memory space command.
            /// </summary>
            Memory = 0x2,

            /// <summary>
            ///     Enable bus mastering command.
            /// </summary>
            Master = 0x4,

            /// <summary>
            ///     Enable response to special cycles command.
            /// </summary>
            Special = 0x8,

            /// <summary>
            ///     Use memory write and invalidate command.
            /// </summary>
            Invalidate = 0x10,

            /// <summary>
            ///     Enable palette snooping command.
            /// </summary>
            VGA_Pallete = 0x20,

            /// <summary>
            ///     Enable parity checking command.
            /// </summary>
            Parity = 0x40,

            /// <summary>
            ///     Enable address/data stepping command.
            /// </summary>
            Wait = 0x80,

            /// <summary>
            ///     Enable SERR command.
            /// </summary>
            SERR = 0x100,

            /// <summary>
            ///     Enable back-to-back writes command.
            /// </summary>
            Fast_Back = 0x200
        }

        /// <summary>
        ///     Enumeration of PCI header types.
        /// </summary>
        public enum PCIHeaderType : byte
        {
            /// <summary>
            ///     Indicates a normal PCI devise.
            /// </summary>
            Normal = 0x00,

            /// <summary>
            ///     Indicates a bridge PCI devise.
            /// </summary>
            Bridge = 0x01,

            /// <summary>
            ///     Indicates a cardbus PCI devise.
            /// </summary>
            Cardbus = 0x02
        }

        /// <summary>
        ///     PCI interrupt pin numbers.
        /// </summary>
        public enum PCIInterruptPIN : byte
        {
            /// <summary>
            ///     None.
            /// </summary>
            None = 0x00,

            /// <summary>
            ///     INTA
            /// </summary>
            INTA = 0x01,

            /// <summary>
            ///     INTB
            /// </summary>
            INTB = 0x02,

            /// <summary>
            ///     INTC
            /// </summary>
            INTC = 0x03,

            /// <summary>
            ///     INTD
            /// </summary>
            INTD = 0x04
        }

        /// <summary>
        ///     PCI status masks.
        /// </summary>
        [Flags]
        public enum PCIStatus : uint
        {
            /// <summary>
            ///     Support capability list.
            /// </summary>
            CAP_LIST = 0x10,

            /// <summary>
            ///     Support 66 Mhz PCI 2.1 bus
            /// </summary>
            SUPPORT_66MHZ = 0x20,

            /// <summary>
            ///     Support User Definable Features [obsolete]
            /// </summary>
            UDF = 0x40,

            /// <summary>
            ///     Accept fast-back (back-to-back writes)
            /// </summary>
            FAST_BACK = 0x80,

            /// <summary>
            ///     Detected parity error.
            /// </summary>
            PARITY = 0x100,

            /// <summary>
            ///     DEVSEL timing.
            /// </summary>
            DEVSEL_MASK = 0x600,

            /// <summary>
            ///     DEVSEL timing - fast.
            /// </summary>
            DEVSEL_FAST = 0x000,

            /// <summary>
            ///     DEVSEL timing - medium.
            /// </summary>
            DEVSEL_MEDIUM = 0x200,

            /// <summary>
            ///     DEVSEL timing - slow.
            /// </summary>
            DEVSEL_SLOW = 0x400,

            /// <summary>
            ///     Set on target abort.
            /// </summary>
            SIG_TARGET_ABORT = 0x800,

            /// <summary>
            ///     Master ack off.
            /// </summary>
            REC_TARGET_ABORT = 0x1000,

            /// <summary>
            ///     Set on master abort.
            /// </summary>
            REC_MASTER_ABORT = 0x2000,

            /// <summary>
            ///     Set when we drive SERR.
            /// </summary>
            SIG_SYSTEM_ERROR = 0x4000,

            /// <summary>
            ///     Set on parity error.
            /// </summary>
            DETECTED_PARITY = 0x8000
        }

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
        public ushort VendorID { get; }

        /// <summary>
        ///     The device's DeviceID.
        /// </summary>
        public ushort DeviceID { get; }

        /// <summary>
        ///     Reads / writes the command register.
        /// </summary>
        public PCICommand Command
        {
            get { return (PCICommand)ReadRegister16(0x04); }
            set { WriteRegister16(0x04, (ushort)value); }
        }

        /// <summary>
        ///     Reads / writes the status register.
        /// </summary>
        public PCIStatus Status
        {
            get { return (PCIStatus)ReadRegister16(0x06); }
            set { WriteRegister16(0x06, (ushort)value); }
        }

        /// <summary>
        ///     The device's RevisionID.
        /// </summary>
        public byte RevisionID { get; private set; }

        /// <summary>
        ///     The device's ProgIF.
        /// </summary>
        public byte ProgIF { get; }

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
        public PCIHeaderType HeaderType { get; private set; }

        /// <summary>
        ///     The device's BIST.
        /// </summary>
        public PCIBISTs BIST { get; private set; }

        /// <summary>
        ///     The device's InterruptLine.
        /// </summary>
        public byte InterruptLine { get; private set; }

        /// <summary>
        ///     The device's InterruptPIN.
        /// </summary>
        public PCIInterruptPIN InterruptPIN { get; private set; }

        /// <summary>
        ///     The device's DeviceExists.
        /// </summary>
        public bool DeviceExists { get; private set; }

        /// <summary>
        ///     Initialises a new, generic PCI device.
        /// </summary>
        /// <param name="bus">The PCI bus number.</param>
        /// <param name="slot">The PCI slot number.</param>
        /// <param name="function">The PCI function number.</param>
        /// <param name="name">The human-readable name of the device.</param>
        [NoDebug]
        public PCIDevice(uint bus, uint slot, uint function, String name)
            : base(DeviceGroup.Unkown, DeviceClass.Generic, DeviceSubClass.PCI, name, new uint[6], false)
        {
            this.bus = bus;
            this.slot = slot;
            this.function = function;

#if PCI_TRACE || COMPILER_TRACE
            ushort vendorID = ReadRegister16(0x00);
            VendorID = ReadRegister16(0x00);
            BasicConsole.WriteLine(((Framework.String)"New PCI device. bus(arg)=") + bus + ", bus(fld)=" + this.bus);
            BasicConsole.WriteLine(((Framework.String)"                slot(arg)=") + slot + ", slot(fld)=" + this.slot);
            BasicConsole.WriteLine(((Framework.String)"                func(arg)=") + function + ", func(fld)=" + this.function);
            BasicConsole.WriteLine(((Framework.String)"                vendorID(loc)=") + vendorID + ", vendorID(fld)=" + this.VendorID);
            BasicConsole.DelayOutput(4);
#else
            VendorID = ReadRegister16(0x00);
#endif
            DeviceID = ReadRegister16(0x02);

            RevisionID = ReadRegister8(0x08);
            ProgIF = ReadRegister8(0x09);
            Subclass = ReadRegister8(0x0A);
            ClassCode = ReadRegister8(0x0B);

            CacheLineSize = ReadRegister8(0x0C);
            LatencyTimer = ReadRegister8(0x0D);
            HeaderType = (PCIHeaderType)ReadRegister8(0x0E);
            BIST = (PCIBISTs)ReadRegister8(0x0F);

            InterruptLine = ReadRegister8(0x3C);
            InterruptPIN = (PCIInterruptPIN)ReadRegister8(0x3D);

            DeviceExists = (uint)VendorID != 0xFFFF && (uint)DeviceID != 0xFFFF;

            Info[0] = bus;
            Info[1] = slot;
            Info[2] = function;
            Info[3] = (byte)HeaderType;
            Info[4] = ClassCode;
            Info[5] = Subclass;
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
        ///     Enables or disables memory.
        /// </summary>
        /// <param name="enable">Whether to enable memory or not.</param>
        public void EnableMemory(bool enable)
        {
            ushort command = (ushort)Command;

            ushort flags = 0x0007;

            if (enable)
                command |= flags;
            else
                command &= (ushort)~flags;

            Command = (PCICommand)command;
        }

        /// <summary>
        ///     Gets the size associated with the specified base address register.
        /// </summary>
        /// <param name="bar">The number of the base address register to test.</param>
        /// <returns>The size.</returns>
        protected uint GetSize(byte bar)
        {
            //Calculate register number for specified BAR number
            byte regNum = (byte)(0x10 + bar*4);
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
        ///     Provides device class information for PCI devices.
        /// </summary>
        /// <remarks>
        ///     Device class information found at https://pci-ids.ucw.cz/read/PD/
        /// </remarks>
        public static class DeviceClassInfo
        {
            /// <summary>
            ///     Gets a string that represents the specified PCI device.
            /// </summary>
            /// <param name="device">The device to get a string for.</param>
            /// <returns>The string.</returns>
            public static String GetString(PCIDevice device)
            {
                switch (device.VendorID)
                {
                    case 0x1022: //AMD
                        switch (device.DeviceID)
                        {
                            case 0x2000:
                                return "AMD PCnet LANCE PCI Ethernet Controller";
                        }
                        break;
                    case 0x104B: //Sony
                        switch (device.DeviceID)
                        {
                            case 0x1040:
                                return "Mylex BT958 SCSI Host Adaptor";
                        }
                        break;
                    case 0x1274: //Ensoniq
                        switch (device.DeviceID)
                        {
                            case 0x1371:
                                return "Ensoniq AudioPCI";
                        }
                        break;
                    case 0x15AD: //VMware
                        switch (device.DeviceID)
                        {
                            case 0x0405:
                                return "VMware NVIDIA 9500MGS";
                            case 0x0770:
                                return "VMware Standard Enhanced PCI to USB Host Controller";
                            case 0x0790:
                                return "VMware 6.0 Virtual USB 2.0 Host Controller";
                            case 0x07A0:
                                return "VMware PCI Express Root Port";
                        }
                        break;
                    case 0x8086: //Intel
                        switch (device.DeviceID)
                        {
                            case 0x7190:
                                return "Intel 440BX/ZX AGPset Host Bridge";
                            case 0x7191:
                                return "Intel 440BX/ZX AGPset PCI-to-PCI bridge";
                            case 0x7110:
                                return "Intel PIIX4/4E/4M ISA Bridge";
                            case 0x7112:
                                return "Intel PIIX4/4E/4M USB Interface";
                        }
                        break;
                }

                switch (device.ClassCode)
                {
                    case 0x00:
                        if (device.Subclass == 0x01)
                        {
                            return "VGA-Compatible unclassified device";
                        }
                        return "Non-VGA unclassified device";
                    case 0x01:

                        #region Mass Storage Controllers

                        if (device.Subclass == 0x00)
                        {
                            return "Mass Storage Controller (SCSI)";
                        }
                        if (device.Subclass == 0x01)
                        {
                            return "Mass Storage Controller (IDE)";
                        }
                        if (device.Subclass == 0x02)
                        {
                            return "Mass Storage Controller (Floppy)";
                        }
                        if (device.Subclass == 0x03)
                        {
                            return "Mass Storage Controller (IPI)";
                        }
                        if (device.Subclass == 0x04)
                        {
                            return "Mass Storage Controller (RAID)";
                        }
                        if (device.Subclass == 0x05)
                        {
                            return "Mass Storage Controller (ATA)";
                        }
                        if (device.Subclass == 0x06)
                        {
                            return "Mass Storage Controller (SATA)";
                        }
                        if (device.Subclass == 0x07)
                        {
                            return "Mass Storage Controller (Serial Attached SCSI)";
                        }
                        if (device.Subclass == 0x08)
                        {
                            return "Mass Storage Controller (Non-volatile memory)";
                        }
                        if (device.Subclass == 0x80)
                        {
                            return "Mass Storage Controller (Other)";
                        }
                        return "Mass Storage Controller (Unrecognised)";

                        #endregion

                    case 0x02:
                        return "Network Controller";
                    case 0x03:
                        return "Display Controller";
                    case 0x04:
                        return "Multimedia Controller";
                    case 0x05:
                        return "Memory Controller";
                    case 0x06:
                        return "Bridge Device";
                    case 0x07:
                        return "Simple Communication Controller";
                    case 0x08:
                        return "Base System Peripheral";
                    case 0x09:
                        return "Input Device";
                    case 0x0A:
                        return "Docking Station";
                    case 0x0B:
                        return "Processor";
                    case 0x0C:

                        #region Serial Bus Controllers

                        if (device.Subclass == 0x00)
                        {
                            return "FireWire (IEEE 1394) Controller";
                        }
                        if (device.Subclass == 0x02)
                        {
                            return "ACCESS Bus";
                        }
                        if (device.Subclass == 0x03)
                        {
                            switch (device.ProgIF)
                            {
                                case 0x00:
                                    return "USB Universal Host Controller Interface";
                                case 0x10:
                                    return "USB Open Host Controller Interface";
                                case 0x20:
                                    return "USB Extended Host Controller Interface";
                                case 0x80:
                                    return "USB (Unknown)";
                                case 0xFE:
                                    return "USB (Not host controller)";
                            }
                        }
                        else if (device.Subclass == 0x04)
                        {
                            return "Fibre Channel";
                        }
                        else if (device.Subclass == 0x05)
                        {
                            return "SMBus";
                        }
                        else if (device.Subclass == 0x06)
                        {
                            return "InfiniBand";
                        }
                        else if (device.Subclass == 0x07)
                        {
                            return "IPMI SMIC interface";
                        }
                        else if (device.Subclass == 0x08)
                        {
                            return "SERCOS interface";
                        }
                        else if (device.Subclass == 0x09)
                        {
                            return "CANBUS";
                        }
                        return "Serial Bus Controller";

                        #endregion

                    case 0x0D:
                        return "Wireless Controller";
                    case 0x0E:
                        return "Intelligent I/O Controller";
                    case 0x0F:
                        return "Satellite Communication Controller";
                    case 0x10:
                        return "Encryption/Decryption Controller";
                    case 0x11:
                        return "Data Acquisition and Signal Processing Controller";
                    //case 0xFF:
                    //    return "Unknown device (Spec unspecified)";
                    //default:
                    //    return "Unknown device (Unrecognised)";
                }
                String result = "ClassCode: ";
                result = result + device.ClassCode + "     Subclass: " + device.Subclass + "     ProgIF: " +
                         device.ProgIF;
                return result;
            }
        }

        #region IOReadWrite

        /// <summary>
        ///     Reads a byte from the specified register.
        /// </summary>
        /// <param name="aRegister">The register to read.</param>
        /// <returns>The byte that has been read.</returns>
        [NoDebug]
        public byte ReadRegister8(byte aRegister)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | (uint)(aRegister & 0xFC);
            return (byte)(PCI_IO.AccessPorts(xAddr, true, 1, 0) >> (aRegister % 4 * 8));
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
            PCI_IO.AccessPorts(xAddr, false, 1, value);
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
            return (ushort)(PCI_IO.AccessPorts(xAddr, true, 2, 0) >> (aRegister % 4 * 8));
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
            PCI_IO.AccessPorts(xAddr, false, 2, value);
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
            return (PCI_IO.AccessPorts(xAddr, true, 4, 0) >> (aRegister % 4 * 8));
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
            PCI_IO.AccessPorts(xAddr, false, 4, value);
        }

        #endregion
    }
}