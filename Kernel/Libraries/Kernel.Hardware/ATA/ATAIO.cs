using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.ATA
{
    public class ATAIO : FOS_System.Object
    {
        public readonly IO.IOPort Data;
        //* Error Register: BAR0 + 1; // Read Only
        //* Features Register: BAR0 + 1; // Write Only
        public readonly IO.IOPort SectorCount;
        // ATA_REG_SECCOUNT1  0x08 - HOB
        public readonly IO.IOPort LBA0;
        public readonly IO.IOPort LBA1;
        public readonly IO.IOPort LBA2;
        // ATA_REG_LBA3       0x09 - HOB
        // ATA_REG_LBA4       0x0A - HOB
        // ATA_REG_LBA5       0x0B - HOB
        public readonly IO.IOPort DeviceSelect;
        public readonly IO.IOPort Command;
        public readonly IO.IOPort Status;
        //* Alternate Status Register: BAR1 + 2; // Read Only.
        public readonly IO.IOPort Control;
        //* DEVADDRESS: BAR1 + 2; //Don't know what this register is for

        [Compiler.NoDebug]
        internal ATAIO(bool isSecondary)
        {
            UInt16 xBAR0 = (UInt16)(isSecondary ? 0x0170 : 0x01F0);
            UInt16 xBAR1 = (UInt16)(isSecondary ? 0x0374 : 0x03F4);
            Data = new IO.IOPort(xBAR0);
            SectorCount = new IO.IOPort(xBAR0, 2);
            LBA0 = new IO.IOPort(xBAR0, 3);
            LBA1 = new IO.IOPort(xBAR0, 4);
            LBA2 = new IO.IOPort(xBAR0, 5);
            Command = new IO.IOPort(xBAR0, 7);
            Status = new IO.IOPort(xBAR0, 7);
            DeviceSelect = new IO.IOPort(xBAR0, 6);
            Control = new IO.IOPort(xBAR1, 2);
        }
    }
}
